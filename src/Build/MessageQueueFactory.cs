using System;
using PipServices3.Commons.Config;
using PipServices3.Commons.Refer;
using PipServices3.Components.Build;
using PipServices3.Messaging.Queues;

namespace PipServices3.Messaging.Build
{
    /// <summary>
    /// MessageQueueFactory are creates MessageQueue components by their descriptors.
    /// Name of created message queue is taken from its descriptor.
    /// </summary>
    public abstract class MessageQueueFactory: Factory, IMessageQueueFactory, IConfigurable, IReferenceable
    {
        protected ConfigParams _config;
        protected IReferences _references;

        public MessageQueueFactory()
        {
        }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public virtual void Configure(ConfigParams config)
        {
            _config = config;
        }

        /// <summary>
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
        public virtual void SetReferences(IReferences references)
        {
            _references = references;
        }

        /// <summary>
        /// Creates a message queue component and assigns its name.
        /// </summary>
        /// <param name="name">name of the created message queue</param>
        /// <returns>A created message queue</returns>
        public abstract IMessageQueue CreateQueue(string name);

    }
}
