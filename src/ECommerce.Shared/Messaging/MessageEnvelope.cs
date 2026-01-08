namespace ECommerce.Shared.Messaging;

/// <summary>
/// Represents a message envelope with standardized metadata and payload.
/// </summary>
/// <typeparam name="TPayload">The payload type carried by the envelope.</typeparam>
public sealed record MessageEnvelope<TPayload>
{
    /// <summary>
    /// Gets the unique message identifier.
    /// </summary>
    public Guid MessageId { get; init; }

    /// <summary>
    /// Gets the message type name.
    /// </summary>
    public string MessageType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the correlation identifier for the business flow.
    /// </summary>
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// Gets the identifier of the message that caused this message.
    /// </summary>
    public Guid? CausationId { get; init; }

    /// <summary>
    /// Gets the occurrence time in UTC.
    /// </summary>
    public DateTime OccurredAtUtc { get; init; }

    /// <summary>
    /// Gets the producer service name.
    /// </summary>
    public string Producer { get; init; } = string.Empty;

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Gets the payload for the message.
    /// </summary>
    public TPayload Payload { get; init; } = default!;
}
