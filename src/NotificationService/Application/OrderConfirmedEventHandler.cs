using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application;

/// <summary>
/// Handles order confirmed events by sending notifications.
/// </summary>
public sealed class OrderConfirmedEventHandler : IMessageHandler<OrderConfirmedEvent>
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderConfirmedEventHandler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OrderConfirmedEventHandler(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Handles the specified order confirmed event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<OrderConfirmedEvent> message, CancellationToken cancellationToken)
    {
        await EnsureNotificationAsync(message, "email", "order-confirmed-email", cancellationToken);
        await EnsureNotificationAsync(message, "sms", "order-confirmed-sms", cancellationToken);
    }

    /// <summary>
    /// Ensures a notification log exists and simulates sending the message.
    /// </summary>
    /// <param name="message">The order confirmed envelope.</param>
    /// <param name="channel">The notification channel.</param>
    /// <param name="template">The template name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnsureNotificationAsync(
        MessageEnvelope<OrderConfirmedEvent> message,
        string channel,
        string template,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.NotificationLogs
            .FirstOrDefaultAsync(log =>
                log.OrderId == message.Payload.OrderId &&
                log.Channel == channel &&
                log.Template == template &&
                log.Status == "Sent",
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

        _dbContext.NotificationLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SendNotificationAsync(log, cancellationToken);
    }

    /// <summary>
    /// Simulates a provider call and updates the notification log.
    /// </summary>
    /// <param name="log">The notification log.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SendNotificationAsync(NotificationLog log, CancellationToken cancellationToken)
    {
        log.Attempt++;
        log.Status = "Sent";
        log.ProviderMessageId = Guid.NewGuid().ToString();
        log.ProviderResponseJson = "{\"status\":\"sent\"}";
        log.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
