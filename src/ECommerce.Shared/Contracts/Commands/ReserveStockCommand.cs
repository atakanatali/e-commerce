namespace ECommerce.Shared.Contracts.Commands;

/// <summary>
/// Represents a command to reserve stock for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Items">The items to reserve.</param>
/// <param name="ReservationExpiresAtUtc">The optional reservation expiry time.</param>
public sealed record ReserveStockCommand(
    Guid OrderId,
    IReadOnlyCollection<ReserveStockItem> Items,
    DateTime? ReservationExpiresAtUtc);

/// <summary>
/// Represents a stock item to reserve.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Quantity">The quantity to reserve.</param>
public sealed record ReserveStockItem(Guid ProductId, int Quantity);
