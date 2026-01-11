using Microsoft.EntityFrameworkCore;
using Notification.Worker.Application.Abstractions;
using Notification.Worker.Domain;

namespace Notification.Worker.Infrastructure.Persistence;

/// <summary>
/// Provides EF Core access for notification logs.
/// </summary>
public sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationLogRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public NotificationLogRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<NotificationLog?> GetSentNotificationAsync(
        Guid orderId,
        string channel,
        string template,
        CancellationToken cancellationToken)
    {
        return _dbContext.NotificationLogs.FirstOrDefaultAsync(log =>
            log.OrderId == orderId &&
            log.Channel == channel &&
            log.Template == template &&
            log.Status == "Sent",
            cancellationToken);
    }

    /// <inheritdoc />
    public void Add(NotificationLog log)
    {
        _dbContext.NotificationLogs.Add(log);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
