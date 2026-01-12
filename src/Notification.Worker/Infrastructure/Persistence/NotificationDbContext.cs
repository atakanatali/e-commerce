using ECommerce.Core.Persistence;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Notification.Worker.Domain;

namespace Notification.Worker.Infrastructure.Persistence;

/// <summary>
/// Provides the Entity Framework Core database context for the Notification service.
/// </summary>
public sealed class NotificationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the notification logs set.
    /// </summary>
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    /// <summary>
    /// Gets the outbox messages set.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Gets the inbox messages set.
    /// </summary>
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    /// <summary>
    /// Configures entity mappings for notification logs and shared inbox/outbox tables.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("notification_logs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.VariablesJson).HasColumnType("jsonb");
            entity.Property(log => log.ProviderResponseJson).HasColumnType("jsonb");
            entity.Property(log => log.CreatedAtUtc).HasColumnType("timestamptz");
            entity.Property(log => log.UpdatedAtUtc).HasColumnType("timestamptz");
            entity.HasIndex(log => log.OrderId);
            entity.HasIndex(log => log.Status);
        });

        modelBuilder.ConfigureInboxOutboxMessages();
    }
}
