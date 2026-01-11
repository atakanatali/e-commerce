using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Orchestrator.Api.Application.Abstractions;
using Orchestrator.Api.Domain;

namespace Orchestrator.Api.Application.Orders;

/// <summary>
/// Handles order workflows based on stock outcomes.
/// </summary>
public sealed class OrderWorkflowService : IOrderWorkflowService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxRepository _outboxRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderWorkflowService"/> class.
    /// </summary>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="outboxRepository">The outbox repository.</param>
    public OrderWorkflowService(IOrderRepository orderRepository, IOutboxRepository outboxRepository)
    {
        _orderRepository = orderRepository;
        _outboxRepository = outboxRepository;
    }

    /// <inheritdoc />
    public async Task ConfirmOrderAsync(
        MessageEnvelope<StockReservedEvent> message,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(message.Payload.OrderId, cancellationToken);

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

        _outboxRepository.Add(outbox);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CancelOrderAsync(
        MessageEnvelope<StockReservationFailedEvent> message,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(message.Payload.OrderId, cancellationToken);

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

        _outboxRepository.Add(outbox);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }
}
