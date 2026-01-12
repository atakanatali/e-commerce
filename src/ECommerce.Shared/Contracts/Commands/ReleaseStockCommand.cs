namespace ECommerce.Shared.Contracts.Commands;

/// <summary>
/// Represents a command to release reserved stock for an order.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Items">The items to release.</param>
public sealed record ReleaseStockCommand(Guid OrderId, IReadOnlyCollection<ReleaseStockItem> Items);

/// <summary>
/// Represents a stock item to release.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Quantity">The quantity to release.</param>
public sealed record ReleaseStockItem(Guid ProductId, int Quantity);
