using System.Threading.Tasks;

namespace PipServices.Messaging.Queues
{
    public interface IMessageReceiver
    {
        Task ReceiveMessageAsync(MessageEnvelope envelope, IMessageQueue queue);
    }
}