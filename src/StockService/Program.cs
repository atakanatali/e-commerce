using Microsoft.EntityFrameworkCore;
using StockService.Application;
using StockService.Infrastructure.Consumers;
using StockService.Infrastructure.Messaging;
using StockService.Infrastructure.Outbox;
using StockService.Infrastructure.Persistence;

namespace StockService;

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

        builder.Services.AddDbContext<StockDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        builder.Services.AddSingleton<ITopologyInitializer, StockTopologyInitializer>();
        builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        builder.Services.AddScoped<OrderCreatedEventHandler>();

        builder.Services.AddHostedService<OutboxPublisherHostedService>();
        builder.Services.AddHostedService<OrderEventsConsumerHostedService>();

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<ITopologyInitializer>();
            initializer.Initialize();
        }

        host.Run();
    }
}
