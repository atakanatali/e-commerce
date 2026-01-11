using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;

namespace Stock.Worker.Application;

/// <summary>
/// Coordinates stock reservation workflows.
/// </summary>
public interface IStockReservationService
{
    /// <summary>
    /// Handles an order created message.
    /// </summary>
    /// <param name="message">The order created envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleOrderCreatedAsync(MessageEnvelope<OrderCreatedEvent> message, CancellationToken cancellationToken);
}
