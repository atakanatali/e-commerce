namespace StockService.Domain;

/// <summary>
/// Represents a stock item for a product.
/// </summary>
public sealed class StockItem
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the available quantity.
    /// </summary>
    public int AvailableQty { get; set; }

    /// <summary>
    /// Gets or sets the reserved quantity.
    /// </summary>
    public int ReservedQty { get; set; }

    /// <summary>
    /// Gets or sets the update time in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the optimistic concurrency version.
    /// </summary>
    public int Version { get; set; }
}
