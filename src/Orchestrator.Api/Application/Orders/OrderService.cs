using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.Extensions.Logging;
using Orchestrator.Api.Application.Abstractions;
using Orchestrator.Api.Contracts;
using Orchestrator.Api.Domain;

namespace Orchestrator.Api.Application.Orders;

/// <summary>
/// Provides application workflows for orders.
/// </summary>
public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderService"/> class.
    /// </summary>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="outboxRepository">The outbox repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger instance.</param>
    public OrderService(
        IOrderRepository orderRepository,
        IOutboxRepository outboxRepository,
        IOrderUnitOfWork unitOfWork,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Creates order
    /// </summary>
    /// <param name="request">request model</param>
    /// <param name="cancellationToken">token</param>
    /// <returns></returns>
    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
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

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _orderRepository.Add(order);
            _outboxRepository.Add(outbox);
            await _orderRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Failed to create order {OrderId} for user {UserId} during transaction commit.",
                orderId,
                request.UserId);
            throw;
        }

        return new CreateOrderResponse(orderId, order.Status);
    }
}
