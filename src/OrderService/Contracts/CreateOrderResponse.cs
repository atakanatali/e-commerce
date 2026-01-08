namespace OrderService.Contracts;

/// <summary>
/// Represents the response for a created order.
/// </summary>
public sealed record CreateOrderResponse(Guid OrderId, string Status);
