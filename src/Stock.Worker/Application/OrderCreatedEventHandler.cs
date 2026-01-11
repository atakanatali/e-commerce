using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.EntityFrameworkCore;
using Stock.Worker.Domain;
using Stock.Worker.Infrastructure.Outbox;
using Stock.Worker.Infrastructure.Persistence;

namespace Stock.Worker.Application;

/// <summary>
/// Handles order created events by attempting stock reservation.
/// </summary>
public sealed class OrderCreatedEventHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly StockDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCreatedEventHandler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OrderCreatedEventHandler(StockDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Handles the specified order created event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<OrderCreatedEvent> message, CancellationToken cancellationToken)
    {
        var existingReservations = await _dbContext.StockReservations
            .Where(reservation => reservation.OrderId == message.Payload.OrderId)
            .ToListAsync(cancellationToken);

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
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var failure = false;

        foreach (var item in items)
        {
            var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE stock_items SET available_qty = available_qty - {item.Quantity}, reserved_qty = reserved_qty + {item.Quantity}, updated_at_utc = {now}, version = version + 1 WHERE product_id = {item.ProductId} AND available_qty >= {item.Quantity}",
                cancellationToken);

            if (affectedRows == 0)
            {
                failure = true;
                break;
            }
        }

        if (failure)
        {
            await transaction.RollbackAsync(cancellationToken);
            await PublishStockFailedAsync(message, "InsufficientStock", cancellationToken);
            return;
        }

        foreach (var item in items)
        {
            _dbContext.StockReservations.Add(new StockReservation
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
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Publishes a stock reserved event to the outbox.
    /// </summary>
    /// <param name="message">The original order created envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task PublishStockReservedAsync(MessageEnvelope<OrderCreatedEvent> message, CancellationToken cancellationToken)
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

        _dbContext.OutboxMessages.Add(outbox);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Publishes a stock reservation failed event to the outbox.
    /// </summary>
    /// <param name="message">The original order created envelope.</param>
    /// <param name="reason">The failure reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
            _dbContext.StockReservations.Add(new StockReservation
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

        _dbContext.OutboxMessages.Add(outbox);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Builds stock result items for event payloads.
    /// </summary>
    /// <param name="items">The order items.</param>
    /// <param name="success">Whether the reservation succeeded.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting stock items.</returns>
    private async Task<IReadOnlyCollection<StockResultItem>> BuildStockResultItemsAsync(
        IReadOnlyCollection<OrderItemDto> items,
        bool success,
        CancellationToken cancellationToken)
    {
        var results = new List<StockResultItem>();

        foreach (var item in items)
        {
            var stock = await _dbContext.StockItems
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.ProductId == item.ProductId, cancellationToken);

            var available = stock?.AvailableQty ?? 0;
            results.Add(new StockResultItem(item.ProductId, item.Quantity, available, success));
        }

        return results;
    }

    /// <summary>
    /// Creates an outbox message from the specified envelope.
    /// </summary>
    /// <typeparam name="TMessage">The message payload type.</typeparam>
    /// <param name="envelope">The message envelope.</param>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <returns>The outbox message entity.</returns>
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
