using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;
using NotificationService.Infrastructure.Inbox;
using NotificationService.Infrastructure.Outbox;

namespace NotificationService.Infrastructure.Persistence;

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
    /// Configures the database model.
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

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(message => message.Id);
            entity.HasIndex(message => message.ProcessedAtUtc)
                .HasDatabaseName("idx_outbox_unprocessed");
            entity.HasIndex(message => message.MessageId).IsUnique();
            entity.Property(message => message.PayloadJson).HasColumnType("jsonb");
            entity.Property(message => message.OccurredAtUtc).HasColumnType("timestamptz");
            entity.Property(message => message.ProcessedAtUtc).HasColumnType("timestamptz");
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(message => message.MessageId);
            entity.HasIndex(message => message.Status).HasDatabaseName("idx_inbox_status");
            entity.Property(message => message.ReceivedAtUtc).HasColumnType("timestamptz");
            entity.Property(message => message.ProcessedAtUtc).HasColumnType("timestamptz");
        });
    }
}
