namespace Stock.Worker.Domain;

/// <summary>
/// Represents a transactional outbox message.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Gets or sets the outbox identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing key.
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the causation identifier.
    /// </summary>
    public Guid? CausationId { get; set; }

    /// <summary>
    /// Gets or sets the occurrence time in UTC.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the producer service name.
    /// </summary>
    public string Producer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the payload JSON.
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processed time in UTC.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the last error.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the lock expiry time in UTC.
    /// </summary>
    public DateTime? LockedUntilUtc { get; set; }

    /// <summary>
    /// Gets or sets the lock owner identifier.
    /// </summary>
    public string? LockedBy { get; set; }
}
