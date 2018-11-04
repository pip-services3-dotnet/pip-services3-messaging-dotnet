namespace PipServices3.Messaging.Queues
{
    /// <summary>
    /// Data object that contains supported capabilities of a message queue.
    /// If certain capability is not supported a queue will throw NotImplemented exception.
    /// </summary>
    public class MessagingCapabilities
    {
        /// <summary>
        /// Creates a new instance of the capabilities object.
        /// </summary>
        /// <param name="canMessageCount">true if queue supports reading message count.</param>
        /// <param name="canSend">true if queue is able to send messages.</param>
        /// <param name="canReceive">true if queue is able to receive messages.</param>
        /// <param name="canPeek">true if queue is able to peek messages.</param>
        /// <param name="canPeekBatch">true if queue is able to peek multiple messages in one batch.</param>
        /// <param name="canRenewLock">true if queue is able to renew message lock.</param>
        /// <param name="canAbandon">true if queue is able to abandon messages.</param>
        /// <param name="canDeadLetter">true if queue is able to send messages to dead letter queue.</param>
        /// <param name="canClear">true if queue can be cleared.</param>
        public MessagingCapabilities(
            bool canMessageCount, bool canSend, bool canReceive, bool canPeek, bool canPeekBatch,
            bool canRenewLock, bool canAbandon, bool canDeadLetter, bool canClear
        )
        {
            CanMessageCount = canMessageCount;
            CanSend = canSend;
            CanReceive = canReceive;
            CanPeek = canPeek;
            CanPeekBatch = canPeekBatch;
            CanRenewLock = canRenewLock;
            CanAbandon = canAbandon;
            CanDeadLetter = canDeadLetter;
            CanClear = canClear;
        }

        /// <summary>
        /// Informs if the queue is able to read number of messages.
        /// </summary>
        public bool CanMessageCount { get; private set; }
        /// <summary>
        /// Informs if the queue is able to send messages.
        /// </summary>
        public bool CanSend { get; private set; }
        /// <summary>
        /// Informs if the queue is able to receive messages.
        /// </summary>
        public bool CanReceive { get; private set; }
        /// <summary>
        /// Informs if the queue is able to peek messages.
        /// </summary>
        public bool CanPeek { get; private set; }
        /// <summary>
        /// Informs if the queue is able to peek multiple messages in one batch.
        /// </summary>
        public bool CanPeekBatch { get; private set; }
        /// <summary>
        /// Informs if the queue is able to renew message lock.
        /// </summary>
        public bool CanRenewLock { get; private set; }
        /// <summary>
        /// Informs if the queue is able to abandon messages.
        /// </summary>
        public bool CanAbandon { get; private set; }
        /// <summary>
        /// Informs if the queue is able to send messages to dead letter queue.
        /// </summary>
        public bool CanDeadLetter { get; private set; }
        /// <summary>
        /// Informs if the queue can be cleared.
        /// </summary>
        public bool CanClear { get; private set; }
    }
}