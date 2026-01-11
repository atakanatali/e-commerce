using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Stock.Worker.Application.Abstractions;
using Stock.Worker.Domain;

namespace Stock.Worker.Application;

/// <summary>
/// Handles stock reservation workflows for incoming orders.
/// </summary>
public sealed class StockReservationService : IStockReservationService
{
    private readonly IStockRepository _stockRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockReservationService"/> class.
    /// </summary>
    /// <param name="stockRepository">The stock repository.</param>
    public StockReservationService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    /// <inheritdoc />
    public async Task HandleOrderCreatedAsync(
        MessageEnvelope<OrderCreatedEvent> message,
        CancellationToken cancellationToken)
    {
        var existingReservations = await _stockRepository.GetReservationsAsync(
            message.Payload.OrderId,
            cancellationToken);

        if (existingReservations.Any(reservation => reservation.Status == "Reserved"))
        {
            await PublishStockReservedAsync(message, cancellationToken);
            return;
        }

        if (existingReservations.Any(reservation => reservation.Status == "Failed"))
        {
            await PublishStockFailedAsync(message, "InsufficientStock", cancellationToken);
            return;
        }

        var now = DateTime.UtcNow;
        var items = message.Payload.Items.ToList();
        await using var transaction = await _stockRepository.BeginTransactionAsync(cancellationToken);
        var reserved = await _stockRepository.TryReserveAsync(items, now, cancellationToken);

        if (!reserved)
        {
            await transaction.RollbackAsync(cancellationToken);
            await PublishStockFailedAsync(message, "InsufficientStock", cancellationToken);
            return;
        }

        foreach (var item in items)
        {
            _stockRepository.AddReservation(new StockReservation
            {
                ReservationId = Guid.NewGuid(),
                OrderId = message.Payload.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Status = "Reserved",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await PublishStockReservedAsync(message, cancellationToken);
        await _stockRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task PublishStockReservedAsync(
        MessageEnvelope<OrderCreatedEvent> message,
        CancellationToken cancellationToken)
    {
        var items = await BuildStockResultItemsAsync(message.Payload.Items, success: true, cancellationToken);
        var reservedEvent = new StockReservedEvent(message.Payload.OrderId, items);

        var envelope = new MessageEnvelope<StockReservedEvent>
        {
            MessageId = Guid.NewGuid(),
            MessageType = nameof(StockReservedEvent),
            CorrelationId = message.Payload.OrderId,
            CausationId = message.MessageId,
            OccurredAtUtc = DateTime.UtcNow,
            Producer = "stock-service",
            Version = 1,
            Payload = reservedEvent
        };

        var outbox = CreateOutboxMessage(
            envelope,
            TopologyConstants.EventsExchangeName,
            TopologyConstants.EventRoutingKeys.StockReserved);

        _stockRepository.AddOutboxMessage(outbox);
        await _stockRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task PublishStockFailedAsync(
        MessageEnvelope<OrderCreatedEvent> message,
        string reason,
        CancellationToken cancellationToken)
    {
        var items = await BuildStockResultItemsAsync(message.Payload.Items, success: false, cancellationToken);
        var failedEvent = new StockReservationFailedEvent(message.Payload.OrderId, reason, items);

        var envelope = new MessageEnvelope<StockReservationFailedEvent>
        {
            MessageId = Guid.NewGuid(),
            MessageType = nameof(StockReservationFailedEvent),
            CorrelationId = message.Payload.OrderId,
            CausationId = message.MessageId,
            OccurredAtUtc = DateTime.UtcNow,
            Producer = "stock-service",
            Version = 1,
            Payload = failedEvent
        };

        var outbox = CreateOutboxMessage(
            envelope,
            TopologyConstants.EventsExchangeName,
            TopologyConstants.EventRoutingKeys.StockReserveFailed);

        foreach (var item in message.Payload.Items)
        {
            _stockRepository.AddReservation(new StockReservation
            {
                ReservationId = Guid.NewGuid(),
                OrderId = message.Payload.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Status = "Failed",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }

        _stockRepository.AddOutboxMessage(outbox);
        await _stockRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<StockResultItem>> BuildStockResultItemsAsync(
        IReadOnlyCollection<OrderItemDto> items,
        bool success,
        CancellationToken cancellationToken)
    {
        var results = new List<StockResultItem>();

        foreach (var item in items)
        {
            var stock = await _stockRepository.GetStockItemAsync(item.ProductId, cancellationToken);

            var available = stock?.AvailableQty ?? 0;
            results.Add(new StockResultItem(item.ProductId, item.Quantity, available, success));
        }

        return results;
    }

    private static OutboxMessage CreateOutboxMessage<TMessage>(
        MessageEnvelope<TMessage> envelope,
        string exchange,
        string routingKey)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = envelope.MessageId,
            MessageType = envelope.MessageType,
            Exchange = exchange,
            RoutingKey = routingKey,
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            OccurredAtUtc = envelope.OccurredAtUtc,
            Producer = envelope.Producer,
            Version = envelope.Version,
            PayloadJson = JsonSerializer.Serialize(envelope)
        };
    }
}
