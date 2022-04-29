using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipServices3.Commons.Config;
using PipServices3.Commons.Run;

namespace PipServices3.Messaging.Queues
{
    /// <summary>
    /// Message queue that caches received messages in memory to allow peek operations
    /// that may not be supported by the undelying queue.
    /// 
    /// This queue is users as a base implementation for other queues
    /// </summary>
    public abstract class CachedMessageQueue : MessageQueue, ICleanable
    {
        protected ManualResetEvent _receiveEvent = new ManualResetEvent(false);
        protected Queue<MessageEnvelope> _messages = new Queue<MessageEnvelope>();
        protected bool _autoSubscribe;
        protected IMessageReceiver _receiver;

        /// <summary>
        /// Creates a new instance of the message queue.
        /// </summary>
        /// <param name="name">(optional) a queue name.</param>
        /// See <see cref="MessagingCapabilities"/>
        public CachedMessageQueue(string name = null, MessagingCapabilities capabilities = null)
            : base(name, capabilities)
        { }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public override void Configure(ConfigParams config)
        {
            base.Configure(config);

            _autoSubscribe = config.GetAsBooleanWithDefault("options.autosubscribe", _autoSubscribe);
        }

        /// <summary>
        /// Opens the component with given connection and credential parameters.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="connections">connection parameters</param>
        /// <param name="credential">credential parameters</param>
        public async override Task OpenAsync(string correlationId)
        {
            if (IsOpen())
            {
                return;
            }

            try
            {
                // Autosubscribe right away
                if (_autoSubscribe)
                {
                    await SubscribeAsync(correlationId);
                }
            }
            catch (Exception ex)
            {
                await CloseAsync(correlationId);
                throw ex;
            }

            _logger.Debug(correlationId, "Opened queue {0}", this.Name);
        }

        /// <summary>
        /// Closes component and frees used resources.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public override async Task CloseAsync(string correlationId)
        {
            if (!IsOpen())
            {
                return;
            }

            // Unsubscribe from broker
            await UnsubscribeAsync(correlationId);

            _receiveEvent.Set();

            _logger.Debug(correlationId, "Closed queue {0}", this.Name);
        }

        /// <summary>
        /// Subscribes to the message broker.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        protected abstract Task SubscribeAsync(string correlationId);

        /// <summary>
        /// Unsubscribes from the message broker.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        protected abstract Task UnsubscribeAsync(string correlationId);

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
            }

            _logger.Debug(correlationId, "Cleared queue {0}", this.Name);

            await Task.Delay(0);
        }

        /// <summary>
        /// Gets the current number of messages in the queue to be delivered.
        /// </summary>
        public override async Task<long> ReadMessageCountAsync()
        {
            var messageCount = 0;
            lock (_lock)
            {
                messageCount = _messages.Count;
            }
            return await Task.FromResult<long>(messageCount);
        }

        /// <summary>
        /// Peeks a single incoming message from the queue without removing it. If there
        /// are no messages available in the queue it returns null.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns>a message envelop object.</returns>
        public override async Task<MessageEnvelope> PeekAsync(string correlationId)
        {
            // Check if queue is opened
            CheckOpen(correlationId);

            // Subscribe if needed
            await SubscribeAsync(correlationId);

            MessageEnvelope message = null;

            lock (_lock)
            {
                // Pick a message
                if (_messages.Count > 0)
                    message = _messages.Peek();
            }

            if (message != null)
            {
                _logger.Trace(message.CorrelationId, "Peeked message {0} on {1}", message, this.Name);
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
            // Check if queue is opened
            CheckOpen(correlationId);

            // Subscribe if needed
            await SubscribeAsync(correlationId);

            List<MessageEnvelope> messages = null;

            lock (_lock)
            {
                messages = _messages.ToArray().Take(messageCount).ToList();
            }

            _logger.Trace(correlationId, "Peeked {0} messages on {1}", messages.Count, this.Name);

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
            // Check if queue is opened
            CheckOpen(correlationId);

            // Subscribe if needed
            await SubscribeAsync(correlationId);

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
            }

            return message;
        }

        public override async Task ListenAsync(string correlationId, IMessageReceiver receiver)
        {
            // Check if queue is opened
            CheckOpen(correlationId);

            // Subscribe if needed
            await SubscribeAsync(correlationId);

            _logger.Debug(null, "Started listening messages at {0}", this.Name);

            // Resend collected messages to receiver
            IEnumerable<MessageEnvelope> batchMessages;

            lock (_lock)
            {
                batchMessages = _messages.ToList();
                _messages.Clear();
            }

            // Resend collected messages to receiver
            foreach (var message in batchMessages)
            {
                await receiver.ReceiveMessageAsync(message, this);
            }

            // Set the receiver
            lock (_lock)
            {
                _receiver = receiver;
            }
        }

        /// <summary>
        /// Ends listening for incoming messages. When this method is call listen() 
        /// unblocks the thread and execution continues.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        public override void EndListen(string correlationId)
        {
            lock (_lock)
            {
                _receiver = null;
            }
        }

    }
}
