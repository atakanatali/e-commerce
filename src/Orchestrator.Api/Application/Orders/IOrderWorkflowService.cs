using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;

namespace Orchestrator.Api.Application.Orders;

/// <summary>
/// Coordinates order status workflows.
/// </summary>
public interface IOrderWorkflowService
{
    /// <summary>
    /// Confirms an order when stock is reserved.
    /// </summary>
    /// <param name="message">The stock reserved message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ConfirmOrderAsync(MessageEnvelope<StockReservedEvent> message, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels an order when stock reservation fails.
    /// </summary>
    /// <param name="message">The stock reservation failed message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CancelOrderAsync(MessageEnvelope<StockReservationFailedEvent> message, CancellationToken cancellationToken);
}
