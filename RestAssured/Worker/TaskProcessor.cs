using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace RestAssured.Worker
{
    /// <summary>
    /// Queue based sequential task processor.
    /// </summary>
    public sealed class TaskProcessor
    {
        private const int MinQueuePollingIntervalInMilliseconds = 250;
        private readonly ConcurrentQueue<WorkerTask> tasks;
        private readonly ILogger<TaskProcessor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskProcessor"/> class.
        /// </summary>
        /// <param name="tasks">Task queue.</param>
        /// <param name="queuePollingTime">Polling interval between dequeue calls.</param>
        /// <param name="logger">Logger instance.</param>
        public TaskProcessor(ConcurrentQueue<WorkerTask> tasks, TimeSpan queuePollingTime, ILogger<TaskProcessor> logger)
        {
            this.tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (queuePollingTime.TotalMilliseconds < MinQueuePollingIntervalInMilliseconds)
            {
                QueuePollingInterval = TimeSpan.FromMilliseconds(MinQueuePollingIntervalInMilliseconds);
                logger.LogWarning($"Configured task queue polling interval {queuePollingTime.TotalMilliseconds} ms" +
                                    $" is lower than minimum required {MinQueuePollingIntervalInMilliseconds} ms. " +
                                        $"Defaulting to {MinQueuePollingIntervalInMilliseconds} ms.");
            }
            else
            {
                QueuePollingInterval = queuePollingTime;
            }
        }

        /// <summary>
        /// Gets externally visible queue polling interval as it can be overriden by the class.
        /// </summary>
        public TimeSpan QueuePollingInterval { get; }

        /// <summary>
        /// Dequeues and process tasks in a sequence.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Add parallism here if we need to process tasks in parallel (which we may need).
            while (!cancellationToken.IsCancellationRequested)
            {
                logger.LogTrace($"{tasks.Count} task(s) found in the queue.");

                // Dequeue all tasks in the queue currently available.
                while (tasks.TryDequeue(out WorkerTask task))
                {
                    // Start task and let the task code manage the resilience and logging.
                    _ = task.ExecuteAsync(cancellationToken);
                }

                logger.LogTrace($"Polling again in {QueuePollingInterval.TotalMilliseconds} ms...");
                await Task.Delay(QueuePollingInterval);
            }

            if (tasks.Count > 0)
            {
                logger.LogWarning($"{tasks.Count} task(s) left unprocessed in the queue. Exiting {GetType().Name}...");
            }
        }
    }
}