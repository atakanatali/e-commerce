using ECommerce.Messaging.RabbitMq;
using ECommerce.Core.Persistence;
using ECommerce.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Notification.Worker.Application;
using Notification.Worker.Application.Abstractions;
using Notification.Worker.Infrastructure.Consumers;
using Notification.Worker.Infrastructure.Messaging;
using Notification.Worker.Infrastructure.Outbox;
using Notification.Worker.Infrastructure.Persistence;
using Serilog;

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
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddLogging(builder.Configuration, "notification-worker");
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            LoggingServiceCollectionExtensions.ConfigureSerilog(
                loggerConfiguration,
                context.Configuration,
                context.HostingEnvironment.EnvironmentName,
                services,
                "notification-worker"));

        builder.Services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Default"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                    npgsqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory_Notification",
                        schema: null);
                    //npgsqlOptions.EnableRetryOnFailure();
                }).ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.Services.AddMessageBroker(builder.Configuration);
        builder.Services.AddSingleton<ITopologyInitializer, NotificationTopologyInitializer>();

        builder.Services.AddScoped<OrderConfirmedEventHandler>();

        builder.Services.AddHostedService<OutboxPublisherHostedService>();
        builder.Services.AddHostedService<OrderConfirmedConsumerHostedService>();

        var host = builder.Build();

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
