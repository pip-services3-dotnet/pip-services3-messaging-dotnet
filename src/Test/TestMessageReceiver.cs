using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipServices3.Commons.Run;
using PipServices3.Messaging.Queues;

namespace PipServices3.Messaging.Test
{
    public class TestMessageReceiver: IMessageReceiver, ICleanable
    {
        private List<MessageEnvelope> _messages = new List<MessageEnvelope>();

        public TestMessageReceiver()
        {
        }

        /// <summary>
        ///  Gets the list of received messages.
        /// </summary>
        public List<MessageEnvelope> Messages
        {
            get { return _messages; }
        }

        /// <summary>
        /// Gets the received message count.
        /// </summary>
        public int MessageCount
        {
            get { return _messages.Count; }
        }

        /// <summary>
        /// Receives incoming message from the queue.
        /// </summary>
        /// <param name="envelope">an incoming message</param>
        /// <param name="queue">a queue where the message comes from</param>
        /// See <see cref="MessageEnvelope"/>, <see cref="IMessageQueue"/>
        public async Task ReceiveMessageAsync(MessageEnvelope message, IMessageQueue queue)
        {
            _messages.Add(message);
            await Task.Delay(0);
        }

        /// <summary>
        /// Clears received messages
        /// </summary>
        /// <param name="correlationId"></param>
        public async Task ClearAsync(string correlationId)
        {
            _messages.Clear();
            await Task.Delay(0);
        }
    }
}
