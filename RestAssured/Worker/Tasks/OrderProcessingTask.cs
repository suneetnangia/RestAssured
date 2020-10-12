using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestAssured.Worker.Tasks
{
    /// <summary>
    /// A long running order processing task.
    /// </summary>
    public sealed class OrderProcessingTask : WorkerTask
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderProcessingTask"/> class.
        /// </summary>
        /// <param name="logger">An object implementing the <see cref="ILogger"/> interface.</param>
        public OrderProcessingTask(ILogger logger)
            : base(logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Executes task.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop long running tasks if requested.</param>
        /// <returns>A task.</returns>
        protected override async Task ExecuteTaskAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Executing method {nameof(ExecuteTaskAsync)}");
            await Task.Delay(50000, cancellationToken);
        }
    }
}