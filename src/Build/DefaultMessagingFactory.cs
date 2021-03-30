using PipServices3.Messaging.Queues;
using PipServices3.Components.Build;
using PipServices3.Commons.Refer;

namespace PipServices3.Messaging.Build
{
    /// <summary>
    /// Creates MemoryMessageQueue components by their descriptors.
    /// Name of created message queue is taken from its descriptor.
    /// </summary>
    /// See <a href="https://pip-services3-dotnet.github.io/pip-services3-components-dotnet/class_pip_services_1_1_components_1_1_build_1_1_factory.html">Factory</a>, 
    /// <a href="https://pip-services3-dotnet.github.io/pip-services3-messaging-dotnet/class_pip_services_1_1_messaging_1_1_queues_1_1_memory_message_queue.html">MemoryMessageQueue</a>
    public class DefaultMessagingFactory : Factory
    {
        private static Descriptor MemoryMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "memory", "*", "1.0");
        private static Descriptor MemoryMessageQueue3Descriptor = new Descriptor("pip-services3", "message-queue", "memory", "*", "1.0");
        private static Descriptor MemoryMessageQueueFactoryDescriptor = new Descriptor("pip-services", "queue-factory", "memory", "*", "1.0");
        private static Descriptor MemoryMessageQueueFactory3Descriptor = new Descriptor("pip-services3", "queue-factory", "memory", "*", "1.0");

        /// <summary>
        /// Create a new instance of the factory.
        /// </summary>
        public DefaultMessagingFactory()
        {
            Register(MemoryMessageQueueDescriptor, (locator) =>
            {
                Descriptor descriptor = locator as Descriptor;
                var name = descriptor != null ? descriptor.Name : null;

                return new MemoryMessageQueue(name);
            });
            Register(MemoryMessageQueue3Descriptor, (locator) =>
            {
                Descriptor descriptor = locator as Descriptor;
                var name = descriptor != null ? descriptor.Name : null;

                return new MemoryMessageQueue(name);
            });
            RegisterAsType(MemoryMessageQueueFactoryDescriptor, typeof(MemoryMessageQueueFactory));
            RegisterAsType(MemoryMessageQueueFactory3Descriptor, typeof(MemoryMessageQueueFactory));
        }
    }
}
