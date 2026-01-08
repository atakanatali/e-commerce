namespace ECommerce.Shared.Contracts.Events;

/// <summary>
/// Represents an event raised when a notification is sent.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Channel">The channel name.</param>
/// <param name="To">The recipient.</param>
public sealed record NotificationSentEvent(Guid OrderId, string Channel, string To);

/// <summary>
/// Represents an event raised when a notification fails.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Channel">The channel name.</param>
/// <param name="To">The recipient.</param>
/// <param name="Error">The failure reason.</param>
public sealed record NotificationFailedEvent(Guid OrderId, string Channel, string To, string Error);
