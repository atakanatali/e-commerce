using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using Orchestrator.Api.Application.Orders;

namespace Orchestrator.Api.Application;

/// <summary>
/// Handles stock reserved events by confirming orders.
/// </summary>
public sealed class StockReservedEventHandler : IMessageHandler<StockReservedEvent>
{
    private readonly IOrderWorkflowService _workflowService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockReservedEventHandler"/> class.
    /// </summary>
    /// <param name="workflowService">The order workflow service.</param>
    public StockReservedEventHandler(IOrderWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Handles the specified stock reserved event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<StockReservedEvent> message, CancellationToken cancellationToken)
    {
        await _workflowService.ConfirmOrderAsync(message, cancellationToken);
    }
}
