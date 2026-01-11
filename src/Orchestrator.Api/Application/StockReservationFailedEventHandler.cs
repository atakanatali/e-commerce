using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using Orchestrator.Api.Application.Orders;

namespace Orchestrator.Api.Application;

/// <summary>
/// Handles stock reservation failed events by cancelling orders.
/// </summary>
public sealed class StockReservationFailedEventHandler : IMessageHandler<StockReservationFailedEvent>
{
    private readonly IOrderWorkflowService _workflowService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockReservationFailedEventHandler"/> class.
    /// </summary>
    /// <param name="workflowService">The order workflow service.</param>
    public StockReservationFailedEventHandler(IOrderWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Handles the specified stock reservation failed event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<StockReservationFailedEvent> message, CancellationToken cancellationToken)
    {
        await _workflowService.CancelOrderAsync(message, cancellationToken);
    }
}
