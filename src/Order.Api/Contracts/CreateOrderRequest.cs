namespace Order.Api.Contracts;

/// <summary>
/// Represents a request to create an order.
/// </summary>
public sealed record CreateOrderRequest
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets or sets the order items.
    /// </summary>
    public IReadOnlyCollection<CreateOrderItem> Items { get; init; } = Array.Empty<CreateOrderItem>();
}

/// <summary>
/// Represents an order item in a create order request.
/// </summary>
public sealed record CreateOrderItem
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; init; }
}
