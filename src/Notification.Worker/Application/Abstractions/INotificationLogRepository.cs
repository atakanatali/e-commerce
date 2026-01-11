using Notification.Worker.Domain;

namespace Notification.Worker.Application.Abstractions;

/// <summary>
/// Provides access to notification logs.
/// </summary>
public interface INotificationLogRepository
{
    /// <summary>
    /// Gets a sent notification for the given order and template details.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="template">The template name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notification log or null.</returns>
    Task<NotificationLog?> GetSentNotificationAsync(
        Guid orderId,
        string channel,
        string template,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new notification log.
    /// </summary>
    /// <param name="log">The notification log.</param>
    void Add(NotificationLog log);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
