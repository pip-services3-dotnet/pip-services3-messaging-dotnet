using PipServices.Components.Build;
using PipServices.Commons.Refer;
using PipServices.Messaging.Queues;

namespace PipServices.Messaging.Build
{
    /// <summary>
    /// Creates MemoryMessageQueue components by their descriptors.
    /// Name of created message queue is taken from its descriptor.
    /// </summary>
    /// See <a href="https://rawgit.com/pip-services-dotnet/pip-services-components-dotnet/master/doc/api/class_pip_services_1_1_components_1_1_build_1_1_factory.html">Factory</a>, 
    /// <a href="https://rawgit.com/pip-services-dotnet/pip-services-messaging-dotnet/master/doc/api/class_pip_services_1_1_messaging_1_1_queues_1_1_memory_message_queue.html">MemoryMessageQueue</a>
    public class MemoryMessageQueueFactory : Factory
    {
        public static readonly Descriptor Descriptor = new Descriptor("pip-services", "factory", "message-queue", "memory", "1.0");
        public static readonly Descriptor MemoryQueueDescriptor = new Descriptor("pip-services", "message-queue", "memory", "*", "*");

        public MemoryMessageQueueFactory()
        {
            Register(MemoryQueueDescriptor, (locator) => {
                Descriptor descritor = (Descriptor)locator;
                return new MemoryMessageQueue(descritor.Name);
            });
        }
    }
}
