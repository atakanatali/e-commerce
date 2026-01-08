namespace OrderService.Domain;

/// <summary>
/// Represents an order aggregate.
/// </summary>
public sealed class Order
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the order status.
    /// </summary>
    public string Status { get; set; } = "Created";

    /// <summary>
    /// Gets or sets the total amount.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Gets or sets the creation time in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the update time in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets the order items.
    /// </summary>
    public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
}
