using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;

namespace Notification.Worker.Application;

/// <summary>
/// Coordinates notification workflows.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Handles an order confirmed message.
    /// </summary>
    /// <param name="message">The order confirmed envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleOrderConfirmedAsync(MessageEnvelope<OrderConfirmedEvent> message, CancellationToken cancellationToken);
}
