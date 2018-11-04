using System.Threading.Tasks;

namespace PipServices3.Messaging.Queues
{
    /// <summary>
    /// Callback interface to receive incoming messages.
    /// </summary>
    /// <example>
    /// <code>
    /// class MyMessageReceiver: IMessageReceiver 
    /// {
    ///     public void ReceiveMessage(MessageEnvelop envelop, IMessageQueue queue)
    ///     {
    ///         Console.Out.WriteLine("Received message: " + envelop.getMessageAsString());
    ///         ...
    ///     }
    /// }
    /// 
    /// var messageQueue = new MemoryMessageQueue();
    /// messageQueue.Listen("123", new MyMessageReceiver());
    /// 
    /// messageQueue.Open("123");
    /// messageQueue.Send("123", new MessageEnvelop(null, "mymessage", "ABC")); // Output in console: "ABC"
    /// </code>
    /// </example>
    public interface IMessageReceiver
    {
        /// <summary>
        /// Receives incoming message from the queue.
        /// </summary>
        /// <param name="envelope">an incoming message</param>
        /// <param name="queue">a queue where the message comes from</param>
        /// See <see cref="MessageEnvelope"/>, <see cref="IMessageQueue"/>
        Task ReceiveMessageAsync(MessageEnvelope envelope, IMessageQueue queue);
    }
}