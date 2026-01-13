using ECommerce.Messaging.RabbitMq;
using ECommerce.Core.Persistence;
using ECommerce.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using Stock.Worker.Application;
using Stock.Worker.Application.Abstractions;
using Stock.Worker.Infrastructure.Consumers;
using Stock.Worker.Infrastructure.Messaging;
using Stock.Worker.Infrastructure.Outbox;
using Stock.Worker.Infrastructure.Persistence;

using LoggingServiceCollectionExtensions = ECommerce.Core.Logging.LoggingServiceCollectionExtensions;

namespace Stock.Worker;

/// <summary>
/// Provides the main entry point for the Stock service.
/// </summary>
public static class Program
{
    /// <summary>
    /// Builds and runs the Stock service host.
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
                    "stock-worker");
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(context.Configuration, "stock-worker");

                services.AddDbContext<StockDbContext>(options =>
                    options.UseNpgsql(
                        context.Configuration.GetConnectionString("Default"),
                        npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                            npgsqlOptions.MigrationsHistoryTable(
                                "__EFMigrationsHistory_Stock",
                                schema: null);
                        })
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

                services.AddScoped<IStockRepository, StockRepository>();
                services.AddScoped<IStockReservationService, StockReservationService>();

                services.AddMessageBroker(context.Configuration);
                services.AddSingleton<ITopologyInitializer, StockTopologyInitializer>();

                services.AddScoped<OrderCreatedEventHandler>();

                services.AddHostedService<OutboxPublisherHostedService>();
                services.AddHostedService<OrderEventsConsumerHostedService>();
            })
            .Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("stock-worker");

        WorkerExceptionHandlingExtensions.RegisterGlobalExceptionHandlers(logger, "stock-worker");

        try
        {
            MigrationExtensions.ApplyMigrationsWithRetryAsync<StockDbContext>(host.Services, logger)
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
            logger.LogError(ex, "Worker stock-worker terminated unexpectedly during startup.");
            throw;
        }
    }
}