using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestAssured.Worker
{
    /// <summary>
    /// Base class for worker tasks.
    /// </summary>
    public abstract class WorkerTask
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerTask"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public WorkerTask(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes task in the concrete task class.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the process.</param>
        /// <returns>Task.</returns>
        public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Executing worker task {GetType().Name}...");

            // Use circuit breaker (e.g. Polly) here to make task executions resilient e.g. endpoints busy like Http 503.
            // https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific#general-rest-and-retry-guidelines
            // Errors are logged and should be monitored via external observability tools and Http 202 returned link (later).
            try
            {
                await ExecuteTaskAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unexpected error ocurred in worker task {GetType().Name}...");
                throw;
            }

            logger.LogInformation($"Finished executing worker task {GetType().Name}.");
        }

        /// <summary>
        /// Implements a concrete task.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the process.</param>
        /// <returns>Task.</returns>
        protected abstract Task ExecuteTaskAsync(CancellationToken cancellationToken);
    }
}