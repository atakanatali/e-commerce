using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using Notification.Worker.Application.Abstractions;
using Notification.Worker.Domain;

namespace Notification.Worker.Application;

/// <summary>
/// Provides notification workflows for order events.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationLogRepository _notificationLogRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="notificationLogRepository">The notification log repository.</param>
    public NotificationService(INotificationLogRepository notificationLogRepository)
    {
        _notificationLogRepository = notificationLogRepository;
    }

    /// <inheritdoc />
    public async Task HandleOrderConfirmedAsync(
        MessageEnvelope<OrderConfirmedEvent> message,
        CancellationToken cancellationToken)
    {
        await EnsureNotificationAsync(message, "email", "order-confirmed-email", cancellationToken);
        await EnsureNotificationAsync(message, "sms", "order-confirmed-sms", cancellationToken);
    }

    private async Task EnsureNotificationAsync(
        MessageEnvelope<OrderConfirmedEvent> message,
        string channel,
        string template,
        CancellationToken cancellationToken)
    {
        var existing = await _notificationLogRepository.GetSentNotificationAsync(
            message.Payload.OrderId,
            channel,
            template,
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var variables = new Dictionary<string, string>
        {
            ["orderId"] = message.Payload.OrderId.ToString(),
            ["total"] = message.Payload.Total.ToString("F2")
        };

        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            OrderId = message.Payload.OrderId,
            Channel = channel,
            Recipient = channel == "email" ? "user@example.com" : "+900000000000",
            Template = template,
            VariablesJson = JsonSerializer.Serialize(variables),
            Status = "Pending",
            Attempt = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _notificationLogRepository.Add(log);
        await _notificationLogRepository.SaveChangesAsync(cancellationToken);

        await SendNotificationAsync(log, cancellationToken);
    }

    private async Task SendNotificationAsync(NotificationLog log, CancellationToken cancellationToken)
    {
        log.Attempt++;
        log.Status = "Sent";
        log.ProviderMessageId = Guid.NewGuid().ToString();
        log.ProviderResponseJson = "{\"status\":\"sent\"}";
        log.UpdatedAtUtc = DateTime.UtcNow;

        await _notificationLogRepository.SaveChangesAsync(cancellationToken);
    }
}
