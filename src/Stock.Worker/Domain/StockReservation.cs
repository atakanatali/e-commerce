namespace Stock.Worker.Domain;

/// <summary>
/// Represents a stock reservation.
/// </summary>
public sealed class StockReservation
{
    /// <summary>
    /// Gets or sets the reservation identifier.
    /// </summary>
    public Guid ReservationId { get; set; }

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = "Reserved";

    /// <summary>
    /// Gets or sets the optional expiry time.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creation time in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the update time in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
