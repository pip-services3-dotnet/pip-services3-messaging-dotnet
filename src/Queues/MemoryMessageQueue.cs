using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipServices.Components.Auth;
using PipServices.Components.Connect;

namespace PipServices.Messaging.Queues
{
    /// <summary>
    /// Message queue that sends and receives messages within the same process by using shared memory.
    /// 
    /// This queue is typically used for testing to mock real queues.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// - name:                        name of the message queue
    /// 
    /// ### References ###
    /// 
    /// - *:logger:*:*:1.0           (optional) <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_log_1_1_i_logger.html">ILogger</a> components to pass log messages
    /// - *:counters:*:*:1.0         (optional) <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/interface_pip_services_1_1_components_1_1_count_1_1_i_counters.html">ICounters</a> components to pass collected measurements
    /// </summary>
    /// <example>
    /// <code>
    /// var queue = new MessageQueue("myqueue");
    /// 
    /// queue.Send("123", new MessageEnvelop(null, "mymessage", "ABC"));
    /// 
    /// queue.Receive("123", 0);
    /// </code>
    /// </example>
    /// See <see cref="MessageQueue"/>, <see cref="MessagingCapabilities"/>
    public class MemoryMessageQueue : MessageQueue
    {
        private ManualResetEvent _receiveEvent = new ManualResetEvent(false);
        private Queue<MessageEnvelope> _messages = new Queue<MessageEnvelope>();
        private int _lockTokenSequence = 0;
        private Dictionary<int, LockedMessage> _lockedMessages = new Dictionary<int, LockedMessage>();
        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private bool _opened = false;

        private class LockedMessage
        {
            public MessageEnvelope Message { get; set; }
            public DateTime ExpirationTimeUtc { get; set; }
            public TimeSpan Timeout { get; set; }
        }

        /// <summary>
        /// Creates a new instance of the message queue.
        /// </summary>
        /// <param name="name">(optional) a queue name.</param>
        /// See <see cref="MessagingCapabilities"/>
        public MemoryMessageQueue(string name = null)
            : base(name)
        {
            Name = name;
            Kind = "memory";
            Capabilities = new MessagingCapabilities(true, true, true, true, true, true, true, false, true);
        }

        /// <summary>
        /// Checks if the component is opened.
        /// </summary>
        /// <returns>true if the component has been opened and false otherwise.</returns>
        public override bool IsOpen()
        {
            return _opened;
        }

        /// <summary>
        /// Opens the component with given connection and credential parameters.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="connection">connection parameters</param>
        /// <param name="credential">credential parameters</param>
        public async override Task OpenAsync(string correlationId, ConnectionParams connection, CredentialParams credential)
        {
            _opened = true;
            await Task.Delay(0);
        }

        /// <summary>
        /// Closes component and frees used resources.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public override async Task CloseAsync(string correlationId)
        {
            _opened = false;

            _cancel.Cancel();
            _receiveEvent.Set();

            _logger.Trace(correlationId, "Closed queue {0}", this);

            await Task.Delay(0);
        }

        /// <summary>
        /// Gets the current number of messages in the queue to be delivered.
        /// </summary>
        public override long? MessageCount
        {
            get
            {
                lock (_lock)
                {
                    return _messages.Count;
                }
            }
        }

        /// <summary>
        /// Sends a message into the queue.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="envelope">a message envelop to be sent.</param>
        public override async Task SendAsync(string correlationId, MessageEnvelope message)
        {
            await Task.Yield();
            //await Task.Delay(0);

            lock (_lock)
            {
                message.SentTime = DateTime.UtcNow;

                // Add message to the queue
                _messages.Enqueue(message);
            }

            // Release threads waiting for messages
            _receiveEvent.Set();

            _counters.IncrementOne("queue." + Name + ".sent_messages");
            _logger.Debug(message.CorrelationId, "Sent message {0} via {1}", message, this);
        }

        /// <summary>
        /// Peeks a single incoming message from the queue without removing it. If there
        /// are no messages available in the queue it returns null.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns>a message envelop object.</returns>
        public override async Task<MessageEnvelope> PeekAsync(string correlationId)
        {
            MessageEnvelope message = null;

            lock (_lock)
            {
                // Pick a message
                if (_messages.Count > 0)
                    message = _messages.Peek();
            }

            if (message != null)
            {
                _logger.Trace(message.CorrelationId, "Peeked message {0} on {1}", message, this);
            }

            return await Task.FromResult(message);
        }

        // <summary>
        /// Peeks multiple incoming messages from the queue without removing them. If
        /// there are no messages available in the queue it returns an empty list.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="messageCount">a maximum number of messages to peek.</param>
        /// <returns> list with messages.</returns>
        public override async Task<List<MessageEnvelope>> PeekBatchAsync(string correlationId, int messageCount)
        {
            List<MessageEnvelope> messages = null;

            lock (_lock)
            {
                messages = _messages.ToArray().Take(messageCount).ToList();
            }

            _logger.Trace(correlationId, "Peeked {0} messages on {1}", messages.Count, this);

            return await Task.FromResult(messages);
        }

        /// <summary>
        /// Receives an incoming message and removes it from the queue.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="waitTimeout">a timeout in milliseconds to wait for a message to come.</param>
        /// <returns>a message envelop object.</returns>
        public override async Task<MessageEnvelope> ReceiveAsync(string correlationId, long waitTimeout)
        {
            await Task.Delay(0);

            lock (_lock)
            {
                if (_messages.Count == 0)
                    _receiveEvent.Reset();
                else
                    _receiveEvent.Set();
            }

            _receiveEvent.WaitOne(TimeSpan.FromMilliseconds(waitTimeout));

            MessageEnvelope message = null;

            lock (_lock)
            {
                if (_messages.Count == 0)
                    return null;

                // Get message the the queue
                message = _messages.Dequeue();

                if (message != null)
                {
                    // Generate and set locked token
                    var lockedToken = _lockTokenSequence++;
                    message.Reference = lockedToken;

                    // Add messages to locked messages list
                    var lockedMessage = new LockedMessage
                    {
                        ExpirationTimeUtc = DateTime.UtcNow.AddMilliseconds(waitTimeout),
                        Message = message,
                        Timeout = TimeSpan.FromMilliseconds(waitTimeout)
                    };
                    _lockedMessages.Add(lockedToken, lockedMessage);
                }
            }

            if (message != null)
            {
                _counters.IncrementOne("queue." + Name + ".received_messages");
                _logger.Debug(message.CorrelationId, "Received message {0} via {1}", message, this);
            }

            return message;
        }

        /// <summary>
        /// Renews a lock on a message that makes it invisible from other receivers in
        /// the queue.This method is usually used to extend the message processing time.
        /// </summary>
        /// <param name="message">a message to extend its lock.</param>
        /// <param name="lockTimeout">a locking timeout in milliseconds.</param>
        public override async Task RenewLockAsync(MessageEnvelope message, long lockTimeout)
        {
            if (message.Reference == null) return;

            lock (_lock)
            {
                // Get message from locked queue
                LockedMessage lockedMessage = null;
                int lockedToken = (int)message.Reference;

                // If lock is found, extend the lock
                if (_lockedMessages.TryGetValue(lockedToken, out lockedMessage))
                {
                    // Todo: Shall we skip if the message already expired?
                    if (lockedMessage.ExpirationTimeUtc > DateTime.UtcNow)
                    {
                        lockedMessage.ExpirationTimeUtc = DateTime.UtcNow.Add(lockedMessage.Timeout);
                    }
                }
            }

            _logger.Trace(message.CorrelationId, "Renewed lock for message {0} at {1}", message, this);

            await Task.Delay(0);
        }

        /// <summary>
        /// Returns message into the queue and makes it available for all subscribers to
        /// receive it again.This method is usually used to return a message which could
        /// not be processed at the moment to repeat the attempt.Messages that cause
        /// unrecoverable errors shall be removed permanently or/and send to dead letter queue.
        /// </summary>
        /// <param name="message">a message to return.</param>
        public override async Task AbandonAsync(MessageEnvelope message)
        {
            if (message.Reference == null) return;

            lock (_lock)
            {
                // Get message from locked queue
                int lockedToken = (int)message.Reference;
                LockedMessage lockedMessage = null;
                if (_lockedMessages.TryGetValue(lockedToken, out lockedMessage))
                {
                    // Remove from locked messages
                    _lockedMessages.Remove(lockedToken);
                    message.Reference = null;

                    // Skip if it is already expired
                    if (lockedMessage.ExpirationTimeUtc <= DateTime.UtcNow)
                        return;
                }
                // Skip if it absent
                else return;
            }

            _logger.Trace(message.CorrelationId, "Abandoned message {0} at {1}", message, this);

            // Add back to the queue
            await SendAsync(message.CorrelationId, message);
        }

        /// <summary>
        /// Permanently removes a message from the queue. This method is usually used to
        /// remove the message after successful processing.
        /// </summary>
        /// <param name="message">a message to remove.</param>
        public override async Task CompleteAsync(MessageEnvelope message)
        {
            if (message.Reference == null) return;

            lock (_lock)
            {
                int lockKey = (int)message.Reference;
                _lockedMessages.Remove(lockKey);
                message.Reference = null;
            }

            _logger.Trace(message.CorrelationId, "Completed message {0} at {1}", message, this);

            await Task.Delay(0);
        }

        /// <summary>
        /// Permanently removes a message from the queue and sends it to dead letter queue.
        /// </summary>
        /// <param name="message">a message to be removed.</param>
        public override async Task MoveToDeadLetterAsync(MessageEnvelope message)
        {
            if (message.Reference == null) return;

            lock (_lock)
            {
                int lockKey = (int)message.Reference;
                _lockedMessages.Remove(lockKey);
                message.Reference = null;
            }

            _counters.IncrementOne("queue." + Name + ".dead_messages");
            _logger.Trace(message.CorrelationId, "Moved to dead message {0} at {1}", message, this);

            await Task.Delay(0);
        }

        public override async Task ListenAsync(string correlationId, Func<MessageEnvelope, IMessageQueue, Task> callback)
        {
            _logger.Trace(null, "Started listening messages at {0}", this);

            // Create new token source
            _cancel = new CancellationTokenSource();

            while (!_cancel.Token.IsCancellationRequested)
            {
                var message = await ReceiveAsync(correlationId, 1000);

                if (message != null)
                {
                    try
                    {
                        if (!_cancel.IsCancellationRequested)
                            await callback(message, this);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(correlationId, ex, "Failed to process the message");
                        //await AbandonAsync(message);
                    }
                }
            }
        }

        /// <summary>
        /// Ends listening for incoming messages. When this method is call listen() 
        /// unblocks the thread and execution continues.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public override void EndListen(string correlationId)
        {
            _cancel.Cancel();
        }

        /// <summary>
        /// Clears component state.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns></returns>
        public override async Task ClearAsync(string correlationId)
        {
            lock (_lock)
            {
                // Clear messages
                _messages.Clear();
                _lockedMessages.Clear();
            }

            _logger.Trace(correlationId, "Cleared queue {0}", this);

            await Task.Delay(0);
        }
    }
}
