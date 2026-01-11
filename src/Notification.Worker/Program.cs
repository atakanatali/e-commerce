using ECommerce.Messaging.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Notification.Worker.Application;
using Notification.Worker.Application.Abstractions;
using Notification.Worker.Infrastructure.Consumers;
using Notification.Worker.Infrastructure.Messaging;
using Notification.Worker.Infrastructure.Outbox;
using Notification.Worker.Infrastructure.Persistence;

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

        builder.Services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.Services.AddMessageBroker(builder.Configuration);
        builder.Services.AddSingleton<ITopologyInitializer, NotificationTopologyInitializer>();

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
