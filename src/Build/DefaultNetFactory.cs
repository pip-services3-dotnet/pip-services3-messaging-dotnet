using PipServices.Net.Messaging;
using PipServices.Commons.Build;
using PipServices.Commons.Refer;
using PipServices.Net.Rest;
using PipServices.Net.Status;

namespace PipServices.Net.Build
{
    public class DefaultNetFactory : Factory
    {
        public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "net", "default", "1.0");
        public static Descriptor MemoryMessageQueueFactoryDescriptor = new Descriptor("pip-services", "factory", "message-queue", "memory", "1.0");
        public static Descriptor MemoryMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "memory", "*", "1.0");
        public static Descriptor HttpEndpointDescriptor = new Descriptor("pip-services", "endpoint", "http", "*", "1.0");
        public static Descriptor StatusServiceDescriptor = new Descriptor("pip-services", "status-service", "http", "*", "1.0");
        public static Descriptor HeartbeatServiceDescriptor = new Descriptor("pip-services", "heartbeat-service", "http", "*", "1.0");

        public DefaultNetFactory()
        {
            RegisterAsType(MemoryMessageQueueFactoryDescriptor, typeof(MemoryMessageQueueFactory));
            RegisterAsType(MemoryMessageQueueDescriptor, typeof(MemoryMessageQueue));
            RegisterAsType(HttpEndpointDescriptor, typeof(HttpEndpoint));
            RegisterAsType(StatusServiceDescriptor, typeof(StatusRestService));
            RegisterAsType(HeartbeatServiceDescriptor, typeof(HeartbeatRestService));
        }
    }
}
