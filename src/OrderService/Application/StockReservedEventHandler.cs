using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application;

/// <summary>
/// Handles stock reserved events by confirming orders.
/// </summary>
public sealed class StockReservedEventHandler : IMessageHandler<StockReservedEvent>
{
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockReservedEventHandler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public StockReservedEventHandler(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Handles the specified stock reserved event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<StockReservedEvent> message, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(entity => entity.Id == message.Payload.OrderId, cancellationToken);

        if (order is null || order.Status == "Confirmed")
        {
            return;
        }

        if (order.Status == "Cancelled")
        {
            return;
        }

        order.Status = "Confirmed";
        order.UpdatedAtUtc = DateTime.UtcNow;

        var confirmedEvent = new OrderConfirmedEvent(order.Id, order.UserId, order.Total);
        var envelope = new MessageEnvelope<OrderConfirmedEvent>
        {
            MessageId = Guid.NewGuid(),
            MessageType = nameof(OrderConfirmedEvent),
            CorrelationId = order.Id,
            CausationId = message.MessageId,
            OccurredAtUtc = DateTime.UtcNow,
            Producer = "order-service",
            Version = 1,
            Payload = confirmedEvent
        };

        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = envelope.MessageId,
            MessageType = envelope.MessageType,
            Exchange = TopologyConstants.EventsExchangeName,
            RoutingKey = TopologyConstants.EventRoutingKeys.OrderConfirmed,
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
