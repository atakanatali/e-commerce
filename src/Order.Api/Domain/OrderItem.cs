namespace Order.Api.Domain;

/// <summary>
/// Represents a line item within an order.
/// </summary>
public sealed class OrderItem
{
    /// <summary>
    /// Gets or sets the item identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the ordered quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the line total.
    /// </summary>
    public decimal LineTotal { get; set; }
}
