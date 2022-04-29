using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PipServices3.Commons.Config;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Components.Auth;
using PipServices3.Components.Connect;
using PipServices3.Components.Count;
using PipServices3.Components.Log;

namespace PipServices3.Messaging.Queues
{
    /// <summary>
    /// Abstract message queue that is used as a basis for specific message queue implementations.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// - name:                        name of the message queue
    /// 
    /// connection(s):
    /// - discovery_key:             key to retrieve parameters from discovery service
    /// - protocol:                  connection protocol like http, https, tcp, udp
    /// - host:                      host name or IP address
    /// - port:                      port number
    /// - uri:                       resource URI or connection string with all parameters in it
    /// 
    /// credential(s):
    /// - store_key:                 key to retrieve parameters from credential store
    /// - username:                  user name
    /// - password:                  user password
    /// - access_id:                 application access id
    /// - access_key:                application secret key
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0           (optional) <a href="https://pip-services3-dotnet.github.io/pip-services3-components-dotnet/interface_pip_services_1_1_components_1_1_log_1_1_i_logger.html">ILogger</a> components to pass log messages
    /// - *:counters:*:*:1.0         (optional) <a href="https://pip-services3-dotnet.github.io/pip-services3-components-dotnet/interface_pip_services_1_1_components_1_1_count_1_1_i_counters.html">ICounters</a> components to pass collected measurements
    /// - *:discovery:*:*:1.0        (optional) <a href="https://pip-services3-dotnet.github.io/pip-services3-components-dotnet/interface_pip_services_1_1_components_1_1_connect_1_1_i_discovery.html">IDiscovery</a> components to discover connection(s)
    /// - *:credential-store:*:*:1.0 (optional) <a href="https://pip-services3-dotnet.github.io/pip-services3-components-dotnet/interface_pip_services_1_1_components_1_1_auth_1_1_i_credential_store.html">ICredentialStore</a> componetns to lookup credential(s)
    /// </summary>
    public abstract class MessageQueue : IMessageQueue, IReferenceable, IConfigurable
    {
        protected CompositeLogger _logger = new CompositeLogger();
        protected CompositeCounters _counters = new CompositeCounters();
        protected ConnectionResolver _connectionResolver = new ConnectionResolver();
        protected CredentialResolver _credentialResolver = new CredentialResolver();
        protected object _lock = new object();

        /// <summary>
        /// Creates a new instance of the message queue.
        /// </summary>
        /// <param name="name">(optional) a queue name</param>
        /// <param name="config">configuration parameters</param>
        public MessageQueue(string name = null, MessagingCapabilities capabilities = null)
        {
            Name = name;
            Capabilities = capabilities ?? new MessagingCapabilities(false, false, false, false, false, false, false, false, false);
        }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public virtual void Configure(ConfigParams config)
        {
            _logger.Configure(config);
            _connectionResolver.Configure(config, true);
            _credentialResolver.Configure(config, true);

            Name = NameResolver.Resolve(config, Name);
            Name = config.GetAsStringWithDefault("queue", Name);
        }

        /// <summary>
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
        public virtual void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
            _counters.SetReferences(references);
            _connectionResolver.SetReferences(references);
            _credentialResolver.SetReferences(references);
        }

        /// <summary>
        /// Opens the component.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public async virtual Task OpenAsync(string correlationId)
        {
            var connections = await _connectionResolver.ResolveAllAsync(correlationId);
            if (connections.Count == 0) {
                throw new ConfigException(correlationId, "NO_CONNECTION", "Connection parameters are not set");
            }

            var credential = await _credentialResolver.LookupAsync(correlationId);
            await OpenAsync(correlationId, connections, credential);
        }

        /// <summary>
        /// Checks if the component is opened.
        /// </summary>
        /// <returns>true if the component has been opened and false otherwise.</returns>
        public abstract bool IsOpen();

        /// <summary>
        /// Opens the component with given connection and credential parameters.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public virtual Task OpenAsync(string correlationId, List<ConnectionParams> connections, CredentialParams credential)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the message queue has been opened, and throws an exception if it's not.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        protected void CheckOpen(string correlationId)
        {
            if (!IsOpen())
            {
                throw new InvalidStateException(correlationId, "NOT_OPENED", "The queue is not opened");
            }
        }

        /// <summary>
        /// Closes component and frees used resources.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public abstract Task CloseAsync(string correlationId);
        /// <summary>
        /// Clears component state.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public abstract Task ClearAsync(string correlationId);

        /// <summary>
        /// Gets the queue name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the queue capabilities
        /// </summary>
        public MessagingCapabilities Capabilities { get; protected set; }

        /// <summary>
        /// Reads the current number of messages in the queue to be delivered.
        /// </summary>
        /// <returns>Number of messages to be delivered</returns>
        public abstract Task<long> ReadMessageCountAsync();

        /// <summary>
        /// Sends a message into the queue.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="message">a message envelop to be sent.</param>
        public abstract Task SendAsync(string correlationId, MessageEnvelope message);

        /// <summary>
        /// Sends an object into the queue. Before sending the object is converted into JSON string and wrapped in a MessageEnvelop.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="messageType">a message type</param>
        /// <param name="message">an object value to be sent</param>
        public async Task SendAsObjectAsync(string correlationId, string messageType, object message)
        {
            var envelope = new MessageEnvelope(correlationId, messageType, message);
            await SendAsync(correlationId, envelope);
        }

        /// <summary>
        /// Peeks a single incoming message from the queue without removing it. If there
        /// are no messages available in the queue it returns null.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns>a message envelop object.</returns>
        public abstract Task<MessageEnvelope> PeekAsync(string correlationId);

        /// <summary>
        /// Peeks multiple incoming messages from the queue without removing them. If
        /// there are no messages available in the queue it returns an empty list.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="messageCount">a maximum number of messages to peek.</param>
        /// <returns> list with messages.</returns>
        public abstract Task<List<MessageEnvelope>> PeekBatchAsync(string correlationId, int messageCount);

        /// <summary>
        /// Receives an incoming message and removes it from the queue.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="waitTimeout">a timeout in milliseconds to wait for a message to come.</param>
        /// <returns>a message envelop object.</returns>
        public abstract Task<MessageEnvelope> ReceiveAsync(string correlationId, long waitTimeout);

        /// <summary>
        /// Renews a lock on a message that makes it invisible from other receivers in
        /// the queue.This method is usually used to extend the message processing time.
        /// </summary>
        /// <param name="message">a message to extend its lock.</param>
        /// <param name="lockTimeout">a locking timeout in milliseconds.</param>
        public abstract Task RenewLockAsync(MessageEnvelope message, long lockTimeout);

        /// <summary>
        /// Returns message into the queue and makes it available for all subscribers to
        /// receive it again.This method is usually used to return a message which could
        /// not be processed at the moment to repeat the attempt.Messages that cause
        /// unrecoverable errors shall be removed permanently or/and send to dead letter queue.
        /// </summary>
        /// <param name="message">a message to return.</param>
        public abstract Task AbandonAsync(MessageEnvelope message);

        /// <summary>
        /// Permanently removes a message from the queue. This method is usually used to
        /// remove the message after successful processing.
        /// </summary>
        /// <param name="message">a message to remove.</param>
        public abstract Task CompleteAsync(MessageEnvelope message);

        /// <summary>
        /// Permanently removes a message from the queue and sends it to dead letter queue.
        /// </summary>
        /// <param name="message">a message to be removed.</param>
        public abstract Task MoveToDeadLetterAsync(MessageEnvelope message);

        /// <summary>
        /// Listens for incoming messages and blocks the current thread until queue is closed.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="receiver">a receiver to receive incoming messages.</param>
        public abstract Task ListenAsync(string correlationId, IMessageReceiver receiver);

        public Task ListenAsync(string correlationId, Func<MessageEnvelope, IMessageQueue, Task> callback)
        {
            return ListenAsync(correlationId, new CallbackMessageReceiver(callback));
        }

        /// <summary>
        /// Listens for incoming messages without blocking the current thread.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="receiver">a receiver to receive incoming messages.</param>
        public void BeginListen(string correlationId, IMessageReceiver receiver)
        {
            ThreadPool.QueueUserWorkItem(async delegate {
                await ListenAsync(correlationId, receiver);
            });
        }

        public void BeginListen(string correlationId, Func<MessageEnvelope, IMessageQueue, Task> callback)
        {
            BeginListen(correlationId, new CallbackMessageReceiver(callback));
        }

        /// <summary>
        /// Ends listening for incoming messages. When this method is call listen() 
        /// unblocks the thread and execution continues.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public abstract void EndListen(string correlationId);

        /// <summary>
        /// Gets a string representation of the object.
        /// </summary>
        /// <returns>a string representation of the object.</returns>
        public override string ToString()
        {
            return "[" + Name + "]";
        }
    }
}
