using PipServices.Messaging.Queues;
using PipServices.Components.Build;
using PipServices.Commons.Refer;

namespace PipServices.Messaging.Build
{
    public class DefaultMessagingFactory : Factory
    {
        public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "messaging", "default", "1.0");
        public static Descriptor MemoryMessageQueueFactoryDescriptor = new Descriptor("pip-services", "factory", "message-queue", "memory", "1.0");
        public static Descriptor MemoryMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "memory", "*", "1.0");

        public DefaultMessagingFactory()
        {
            RegisterAsType(MemoryMessageQueueFactoryDescriptor, typeof(MemoryMessageQueueFactory));
            RegisterAsType(MemoryMessageQueueDescriptor, typeof(MemoryMessageQueue));
        }
    }
}
