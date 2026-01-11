using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;

namespace Notification.Worker.Application;

/// <summary>
/// Handles order confirmed events by sending notifications.
/// </summary>
public sealed class OrderConfirmedEventHandler : IMessageHandler<OrderConfirmedEvent>
{
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderConfirmedEventHandler"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    public OrderConfirmedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Handles the specified order confirmed event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<OrderConfirmedEvent> message, CancellationToken cancellationToken)
    {
        await _notificationService.HandleOrderConfirmedAsync(message, cancellationToken);
    }
}
