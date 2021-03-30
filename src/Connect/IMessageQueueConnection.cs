using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipServices3.Messaging.Connect
{
    /// <summary>
    /// Interface for queue connections
    /// </summary>
    public interface IMessageQueueConnection
    {
        /// <summary>
        /// Reads names of available queues or topics.
        /// </summary>
        /// <returns>A list with queue names</returns>
        Task<List<string>> ReadQueueNamesAsync();

        /// <summary>
        /// Create a queue or topic with a specified name.
        /// </summary>
        /// <param name="name">A name of the queue to be created</param>
        Task CreateQueueAsync(string name);

        /// <summary>
        /// Delete a queue or topic with a specified name.
        /// </summary>
        /// <param name="name">A name of the queue to be deleted</param>
        Task DeleteQueueAsync(string name);
    }
}
