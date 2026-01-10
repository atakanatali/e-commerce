namespace Stock.Api.Infrastructure.Inbox;

/// <summary>
/// Represents an inbox message for idempotent consumer processing.
/// </summary>
public sealed class InboxMessage
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the consumer name.
    /// </summary>
    public string Consumer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the handler name.
    /// </summary>
    public string Handler { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the received time in UTC.
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the processing status.
    /// </summary>
    public string Status { get; set; } = "Received";

    /// <summary>
    /// Gets or sets the processed time in UTC.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last error.
    /// </summary>
    public string? LastError { get; set; }
}
