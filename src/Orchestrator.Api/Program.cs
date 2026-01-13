using ECommerce.Messaging.RabbitMq;
using ECommerce.Shared.Messaging.Topology;
using ECommerce.Core.Persistence;
using ECommerce.Core.RateLimiting;
using ECommerce.Core.Redis;
using ECommerce.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Orchestrator.Api.Application;
using Orchestrator.Api.Application.Abstractions;
using Orchestrator.Api.Application.Orders;
using Orchestrator.Api.Infrastructure.Consumers;
using Orchestrator.Api.Infrastructure.Messaging;
using Orchestrator.Api.Infrastructure.Outbox;
using Orchestrator.Api.Infrastructure.Persistence;
using Serilog;
using LoggingServiceCollectionExtensions = ECommerce.Core.Logging.LoggingServiceCollectionExtensions;

namespace Orchestrator.Api;

/// <summary>
/// Provides the main entry point for the Orchestrator service.
/// </summary>
public static class Program
{
    /// <summary>
    /// Builds and runs the Orchestrator service host.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddLogging(builder.Configuration, "orchestrator-api");
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            LoggingServiceCollectionExtensions.ConfigureSerilog(
                loggerConfiguration,
                context.Configuration,
                context.HostingEnvironment.EnvironmentName,
                services,
                "orchestrator-api"));

        builder.Services.AddControllers();
        builder.Services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Default"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
                    npgsqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory_Orchestrator",
                        schema: null);
                    //npgsqlOptions.EnableRetryOnFailure();
                }).ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
        builder.Services.AddScoped<IOrderUnitOfWork, OrderUnitOfWork>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();

        builder.Services.AddMessageBroker(builder.Configuration);
        builder.Services.AddSingleton<ITopologyInitializer, OrderTopologyInitializer>();

        builder.Services.AddRedis(builder.Configuration);
        builder.Services.AddRedisRateLimiting(builder.Configuration);

        builder.Services.AddScoped<StockReservedEventHandler>();
        builder.Services.AddScoped<StockReservationFailedEventHandler>();

        builder.Services.AddHostedService<OutboxPublisherHostedService>();
        builder.Services.AddHostedService<StockEventsConsumerHostedService>();

        var app = builder.Build();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseMiddleware<HttpErrorLoggingMiddleware>();
        app.MapControllers();

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("orchestrator-api");

        MigrationExtensions.ApplyMigrationsWithRetryAsync<OrderDbContext>(app.Services, logger)
            .GetAwaiter()
            .GetResult();

        using (var scope = app.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<ITopologyInitializer>();
            initializer.Initialize();
        }

        app.Run();
    }
}
