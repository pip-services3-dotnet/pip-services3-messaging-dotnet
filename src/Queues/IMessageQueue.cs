using PipServices.Commons.Run;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipServices.Messaging.Queues
{
    public interface IMessageQueue : IOpenable
    {
        string Name { get; }
        MessagingCapabilities Capabilities { get; }
        long? MessageCount { get; }

        Task SendAsync(string correlationId, MessageEnvelope envelope);
        Task SendAsync(string correlationId, string messageType, string message);
        Task SendAsObjectAsync(string correlationId, string messageType, object message);
        Task<MessageEnvelope> PeekAsync(string correlationId);
        Task<List<MessageEnvelope>> PeekBatchAsync(string correlationId, int messageCount);
        Task<MessageEnvelope> ReceiveAsync(string correlationId, long waitTimeout);

        Task RenewLockAsync(MessageEnvelope message, long lockTimeout);
        Task CompleteAsync(MessageEnvelope message);
        Task AbandonAsync(MessageEnvelope message);
        Task MoveToDeadLetterAsync(MessageEnvelope message);

        Task ListenAsync(string correlationId, IMessageReceiver receiver);
        Task ListenAsync(string correlationId, Func<MessageEnvelope, IMessageQueue, Task> callback);
        void BeginListen(string correlationId, IMessageReceiver receiver);
        void BeginListen(string correlationId, Func<MessageEnvelope, IMessageQueue, Task> callback);
        void EndListen(string correlationId);
    }
}