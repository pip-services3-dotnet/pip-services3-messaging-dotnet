using PipServices.Components.Build;
using PipServices.Commons.Refer;

namespace PipServices.Messaging.Queues
{
    public class MemoryMessageQueueFactory : Factory
    {
        public static readonly Descriptor Descriptor = new Descriptor("pip-services-messaging", "factory", "message-queue", "memory", "1.0");
        public static readonly Descriptor MemoryQueueDescriptor = new Descriptor("pip-services-messaging", "message-queue", "memory", "*", "*");

        public MemoryMessageQueueFactory()
        {
            Register(MemoryQueueDescriptor, (locator) => {
                Descriptor descritor = (Descriptor)locator;
                return new MemoryMessageQueue(descritor.Name);
            });
        }
    }
}
