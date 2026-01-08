namespace ECommerce.Shared.Contracts.Events;

/// <summary>
/// Represents an event raised when stock is reserved successfully.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Items">The stock result items.</param>
public sealed record StockReservedEvent(Guid OrderId, IReadOnlyCollection<StockResultItem> Items);

/// <summary>
/// Represents an event raised when stock reservation fails.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Reason">The failure reason.</param>
/// <param name="Items">The stock result items.</param>
public sealed record StockReservationFailedEvent(
    Guid OrderId,
    string Reason,
    IReadOnlyCollection<StockResultItem> Items);

/// <summary>
/// Represents a stock reservation result item.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Requested">The requested quantity.</param>
/// <param name="Available">The available quantity after processing.</param>
/// <param name="Success">Whether the reservation succeeded.</param>
public sealed record StockResultItem(Guid ProductId, int Requested, int Available, bool Success);
