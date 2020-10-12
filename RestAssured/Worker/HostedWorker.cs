using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RestAssured.Worker
{
    /// <summary>
    /// Background service to process long running API requests.
    /// </summary>
    public sealed class HostedWorker : BackgroundService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly TaskProcessor taskProcessor;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedWorker"/> class.
        /// </summary>
        /// <param name="hostApplicationLifetime">IHostApplicationLifetime instance to manage hosted app lifecycle.</param>
        /// <param name="taskProcessor">A long running task processor instance.</param>
        /// <param name="logger">Logger instance.</param>
        public HostedWorker(IHostApplicationLifetime hostApplicationLifetime, TaskProcessor taskProcessor, ILogger<HostedWorker> logger)
        {
            this.hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
            this.taskProcessor = taskProcessor ?? throw new ArgumentNullException(nameof(taskProcessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Start executing background worker.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token.</param>
        /// <returns>An infinitely running task.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Starting {GetType().Name} to process tasks in background...");

            try
            {
                await taskProcessor.ExecuteAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error ocurred in {GetType().Name}.");

                // Exit with code 2 to signify error in background worker service.
                Environment.ExitCode = Constants.BackgroundWorkerUnexpectedFailure;
                hostApplicationLifetime.StopApplication();

                throw;
            }
        }
    }
}