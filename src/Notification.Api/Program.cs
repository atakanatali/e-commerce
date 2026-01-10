using Microsoft.EntityFrameworkCore;
using Notification.Api.Application;
using Notification.Api.Infrastructure.Consumers;
using Notification.Api.Infrastructure.Messaging;
using Notification.Api.Infrastructure.Outbox;
using Notification.Api.Infrastructure.Persistence;

namespace Notification.Api;

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

        builder.Services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
        builder.Services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        builder.Services.AddSingleton<ITopologyInitializer, NotificationTopologyInitializer>();
        builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        builder.Services.AddScoped<OrderConfirmedEventHandler>();

        builder.Services.AddHostedService<OutboxPublisherHostedService>();
        builder.Services.AddHostedService<OrderConfirmedConsumerHostedService>();

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<ITopologyInitializer>();
            initializer.Initialize();
        }

        host.Run();
    }
}
