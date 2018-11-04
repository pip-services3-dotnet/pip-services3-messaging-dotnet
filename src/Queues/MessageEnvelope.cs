using PipServices3.Commons.Convert;
using PipServices3.Commons.Data;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace PipServices3.Messaging.Queues
{
    /// <summary>
    /// Allows adding additional information to messages. A correlation id, message id, and a message type
    /// are added to the data being sent/received.Additionally, a MessageEnvelope can reference a lock token.
    /// 
    /// Side note: a MessageEnvelope's message is stored as a buffer, so strings are converted 
    /// using utf8 conversions.
    /// </summary>
    [DataContract]
    public class MessageEnvelope
    {
        /// <summary>
        /// Creates a new MessageEnvelope.
        /// </summary>
        public MessageEnvelope() { }

        /// <summary>
        /// Creates a new MessageEnvelop, which adds a correlation id, message id, and a
        /// type to the data being sent/received.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="messageType">a string value that defines the message's type.</param>
        /// <param name="message">the data being sent/received.</param>
        public MessageEnvelope(string correlationId, string messageType, string message)
        {
            CorrelationId = correlationId;
            MessageType = messageType;
            Message = message;
            MessageId = IdGenerator.NextLong();
        }

        /// <summary>
        /// Creates a new MessageEnvelop, which adds a correlation id, message id, and a
        /// type to the data being sent/received.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="messageType">a string value that defines the message's type.</param>
        /// <param name="message">the data being sent/received.</param>
        public MessageEnvelope(string correlationId, string messageType, object message)
        {
            CorrelationId = correlationId;
            MessageType = messageType;
            SetMessage(message);
            MessageId = IdGenerator.NextLong();
        }

        /** The stored reference. */
        [IgnoreDataMember]
        public object Reference { get; set; }

        /** The unique business transaction id that is used to trace calls across components. */
        [DataMember]
        public string CorrelationId { get; set; }

        /** The message's auto-generated ID. */
        [DataMember]
        public string MessageId { get; set; }

        /** String value that defines the stored message's type. */
        [DataMember]
        public string MessageType { get; set; }

        /** The time at which the message was sent. */
        [DataMember]
        public DateTime SentTime { get; set; }

        /** The stored message. */
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Stores the given value as a JSON string.
        /// </summary>
        /// <param name="message">the value to convert to JSON and store in this message.</param>
        public void SetMessage(object message)
        {
            Message = JsonConverter.ToJson(message);
        }

        /// <summary>
        /// Gets the value that was stored in this message as a JSON string.
        /// </summary>
        /// <typeparam name="T">the class type</typeparam>
        /// <returns>the value that was stored in this message as a JSON string.</returns>
        public T GetMessage<T>()
        {
            return JsonConverter.FromJson<T>(Message);
        }

        /// <summary>
        /// Convert's this MessageEnvelope to a string, using the following format:
        /// 
        /// <code>"[correlation_id, message_type, message.toString]"</code>.
        /// 
        /// If any of the values are<code>null</code>, they will be replaced with <code>---</code>.
        /// </summary>
        /// <returns>the generated string.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('[');
            builder.Append(CorrelationId ?? "---");
            builder.Append(',');
            builder.Append(MessageType ?? "---");
            builder.Append(',');
            var sample = Message ?? "---";
            sample = sample.Length > 150 ? sample.Substring(0, 150) : sample;
            builder.Append(sample);
            builder.Append(']');
            return builder.ToString();
        }
    }
}