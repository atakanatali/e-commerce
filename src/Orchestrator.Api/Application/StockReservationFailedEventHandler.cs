using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Api.Infrastructure.Outbox;
using Orchestrator.Api.Infrastructure.Persistence;

namespace Orchestrator.Api.Application;

/// <summary>
/// Handles stock reservation failed events by cancelling orders.
/// </summary>
public sealed class StockReservationFailedEventHandler : IMessageHandler<StockReservationFailedEvent>
{
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockReservationFailedEventHandler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public StockReservationFailedEventHandler(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Handles the specified stock reservation failed event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<StockReservationFailedEvent> message, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(entity => entity.Id == message.Payload.OrderId, cancellationToken);

        if (order is null || order.Status == "Cancelled")
        {
            return;
        }

        order.Status = "Cancelled";
        order.UpdatedAtUtc = DateTime.UtcNow;

        var cancelledEvent = new OrderCancelledEvent(order.Id, message.Payload.Reason);
        var envelope = new MessageEnvelope<OrderCancelledEvent>
        {
            MessageId = Guid.NewGuid(),
            MessageType = nameof(OrderCancelledEvent),
            CorrelationId = order.Id,
            CausationId = message.MessageId,
            OccurredAtUtc = DateTime.UtcNow,
            Producer = "order-service",
            Version = 1,
            Payload = cancelledEvent
        };

        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = envelope.MessageId,
            MessageType = envelope.MessageType,
            Exchange = TopologyConstants.EventsExchangeName,
            RoutingKey = TopologyConstants.EventRoutingKeys.OrderCancelled,
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            OccurredAtUtc = envelope.OccurredAtUtc,
            Producer = envelope.Producer,
            Version = envelope.Version,
            PayloadJson = JsonSerializer.Serialize(envelope)
        };

        _dbContext.OutboxMessages.Add(outbox);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
