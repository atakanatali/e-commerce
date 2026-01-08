using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.AspNetCore.Mvc;
using OrderService.Contracts;
using OrderService.Domain;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Controllers;

/// <summary>
/// Provides endpoints for order operations.
/// </summary>
[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OrdersController(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new order and publishes an order created event.
    /// </summary>
    /// <param name="request">The create order request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created order response.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0 || request.Items.Any(item => item.Quantity <= 0))
        {
            return BadRequest("Order items are invalid.");
        }

        var orderId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var total = request.Items.Sum(item => item.Quantity * item.UnitPrice);

        var order = new Order
        {
            Id = orderId,
            UserId = request.UserId,
            Status = "Created",
            Total = total,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var item in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            });
        }

        var createdEvent = new OrderCreatedEvent(
            orderId,
            request.UserId,
            total,
            request.Items.Select(item => new OrderItemDto(item.ProductId, item.Quantity, item.UnitPrice)).ToList());

        var envelope = new MessageEnvelope<OrderCreatedEvent>
        {
            MessageId = Guid.NewGuid(),
            MessageType = nameof(OrderCreatedEvent),
            CorrelationId = orderId,
            CausationId = null,
            OccurredAtUtc = now,
            Producer = "order-service",
            Version = 1,
            Payload = createdEvent
        };

        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = envelope.MessageId,
            MessageType = envelope.MessageType,
            Exchange = TopologyConstants.EventsExchangeName,
            RoutingKey = TopologyConstants.EventRoutingKeys.OrderCreated,
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            OccurredAtUtc = envelope.OccurredAtUtc,
            Producer = envelope.Producer,
            Version = envelope.Version,
            PayloadJson = JsonSerializer.Serialize(envelope)
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.Orders.Add(order);
        _dbContext.OutboxMessages.Add(outbox);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var response = new CreateOrderResponse(orderId, order.Status);
        return CreatedAtAction(nameof(CreateOrder), response);
    }
}
