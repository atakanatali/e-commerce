namespace Notification.Api.Domain;

/// <summary>
/// Represents a notification log entry.
/// </summary>
public sealed class NotificationLog
{
    /// <summary>
    /// Gets or sets the log identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the channel name.
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Template { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the variables JSON.
    /// </summary>
    public string VariablesJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the attempt count.
    /// </summary>
    public int Attempt { get; set; }

    /// <summary>
    /// Gets or sets the provider message identifier.
    /// </summary>
    public string? ProviderMessageId { get; set; }

    /// <summary>
    /// Gets or sets the provider response JSON.
    /// </summary>
    public string? ProviderResponseJson { get; set; }

    /// <summary>
    /// Gets or sets the last error.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the creation time in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the update time in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
