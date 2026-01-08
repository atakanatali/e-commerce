using ECommerce.Shared.Messaging.Topology;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Infrastructure.Consumers;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence;

namespace OrderService;

/// <summary>
/// Provides the main entry point for the Order service.
/// </summary>
public static class Program
{
    /// <summary>
    /// Builds and runs the Order service host.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        builder.Services.AddSingleton<ITopologyInitializer, OrderTopologyInitializer>();
        builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        builder.Services.AddScoped<StockReservedEventHandler>();
        builder.Services.AddScoped<StockReservationFailedEventHandler>();

        builder.Services.AddHostedService<OutboxPublisherHostedService>();
        builder.Services.AddHostedService<StockEventsConsumerHostedService>();

        var app = builder.Build();
        app.MapControllers();

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<ITopologyInitializer>();
            initializer.Initialize();
        });

        app.Run();
    }
}
