using System;
using System.Threading.Tasks;

namespace PipServices3.Messaging.Queues
{
    /// <summary>
    /// Wraps callback into a class with IMessageReceiver interface
    /// </summary>
    public class CallbackMessageReceiver: IMessageReceiver
    {
        private Func<MessageEnvelope, IMessageQueue, Task> _callback;

        public CallbackMessageReceiver(Func<MessageEnvelope, IMessageQueue, Task> callback)
        {
            _callback = callback;
        }

        /// <summary>
        /// Receives incoming message from the queue.
        /// </summary>
        /// <param name="envelope">an incoming message</param>
        /// <param name="queue">a queue where the message comes from</param>
        /// See <see cref="MessageEnvelope"/>, <see cref="IMessageQueue"/>
        public async Task ReceiveMessageAsync(MessageEnvelope envelope, IMessageQueue queue)
        {
            await _callback(envelope, queue);
        }
    }
}
