using Microsoft.Extensions.Logging;

namespace ECommerce.Core.Logging;

/// <summary>
/// Provides helpers for logging unhandled exceptions in worker services.
/// </summary>
public static class WorkerExceptionHandlingExtensions
{
    /// <summary>
    /// Registers global exception handlers for worker processes.
    /// </summary>
    /// <param name="logger">The logger used to record unhandled exceptions.</param>
    /// <param name="workerName">The worker name for log enrichment.</param>
    public static void RegisterGlobalExceptionHandlers(ILogger logger, string workerName)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            logger.LogError(
                exception,
                "Unhandled exception in worker {WorkerName}. Terminating: {IsTerminating}.",
                workerName,
                args.IsTerminating);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            logger.LogError(
                args.Exception,
                "Unobserved task exception in worker {WorkerName}.",
                workerName);
            args.SetObserved();
        };
    }
}
