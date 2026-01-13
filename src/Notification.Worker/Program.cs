using ECommerce.Core.Logging;
using ECommerce.Core.Persistence;
using ECommerce.Messaging.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Worker.Application;
using Notification.Worker.Application.Abstractions;
using Notification.Worker.Infrastructure.Consumers;
using Notification.Worker.Infrastructure.Messaging;
using Notification.Worker.Infrastructure.Outbox;
using Notification.Worker.Infrastructure.Persistence;
using Serilog;

using LoggingServiceCollectionExtensions = ECommerce.Core.Logging.LoggingServiceCollectionExtensions;

namespace Notification.Worker;

/// <summary>
/// Provides the main entry point for the Notification service.
/// </summary>
public static class Program
{
    /// <summary>
    /// Builds and runs the Notification service host.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, loggerConfiguration) =>
            {
                LoggingServiceCollectionExtensions.ConfigureSerilog(
                    loggerConfiguration,
                    context.Configuration,
                    context.HostingEnvironment.EnvironmentName,
                    services,
                    "notification-worker");
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(context.Configuration, "notification-worker");

                services.AddDbContext<NotificationDbContext>(options =>
                    options.UseNpgsql(
                        context.Configuration.GetConnectionString("Default"),
                        npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                            npgsqlOptions.MigrationsHistoryTable(
                                "__EFMigrationsHistory_Notification",
                                schema: null);
                        })
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

                services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
                services.AddScoped<INotificationService, NotificationService>();

                services.AddMessageBroker(context.Configuration);
                services.AddSingleton<ITopologyInitializer, NotificationTopologyInitializer>();

                services.AddScoped<OrderConfirmedEventHandler>();

                services.AddHostedService<OutboxPublisherHostedService>();
                services.AddHostedService<OrderConfirmedConsumerHostedService>();
            })
            .Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("notification-worker");

        WorkerExceptionHandlingExtensions.RegisterGlobalExceptionHandlers(logger, "notification-worker");

        try
        {
            MigrationExtensions.ApplyMigrationsWithRetryAsync<NotificationDbContext>(host.Services, logger)
                .GetAwaiter()
                .GetResult();

            using (var scope = host.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<ITopologyInitializer>();
                initializer.Initialize();
            }

            host.Run();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Worker notification-worker terminated unexpectedly during startup.");
            throw;
        }
    }
}