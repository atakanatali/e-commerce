using ECommerce.Messaging.RabbitMq;
using ECommerce.Core.Persistence;
using ECommerce.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stock.Worker.Application;
using Stock.Worker.Application.Abstractions;
using Stock.Worker.Infrastructure.Consumers;
using Stock.Worker.Infrastructure.Messaging;
using Stock.Worker.Infrastructure.Outbox;
using Stock.Worker.Infrastructure.Persistence;
using Serilog;

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
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddLogging(builder.Configuration, "stock-worker");
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            LoggingServiceCollectionExtensions.ConfigureSerilog(
                loggerConfiguration,
                context.Configuration,
                context.HostingEnvironment.EnvironmentName,
                services,
                "stock-worker"));

        builder.Services.AddDbContext<StockDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Default"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                    npgsqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory_Stock",
                        schema: null);
                    //npgsqlOptions.EnableRetryOnFailure();
                }).ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        builder.Services.AddScoped<IStockRepository, StockRepository>();
        builder.Services.AddScoped<IStockReservationService, StockReservationService>();

        builder.Services.AddMessageBroker(builder.Configuration);
        builder.Services.AddSingleton<ITopologyInitializer, StockTopologyInitializer>();

        builder.Services.AddScoped<OrderCreatedEventHandler>();

        builder.Services.AddHostedService<OutboxPublisherHostedService>();
        builder.Services.AddHostedService<OrderEventsConsumerHostedService>();

        var host = builder.Build();

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
