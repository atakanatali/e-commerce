namespace ECommerce.Shared.Contracts.Events;

/// <summary>
/// Represents an event raised when an order is created.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="Total">The total price.</param>
/// <param name="Items">The order items.</param>
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    IReadOnlyCollection<OrderItemDto> Items);

/// <summary>
/// Represents an event raised when an order is confirmed.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="Total">The total price.</param>
public sealed record OrderConfirmedEvent(Guid OrderId, Guid UserId, decimal Total);

/// <summary>
/// Represents an event raised when an order is cancelled.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Reason">The cancellation reason.</param>
public sealed record OrderCancelledEvent(Guid OrderId, string Reason);

/// <summary>
/// Represents an order item data transfer object.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Quantity">The ordered quantity.</param>
/// <param name="UnitPrice">The unit price.</param>
public sealed record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);
