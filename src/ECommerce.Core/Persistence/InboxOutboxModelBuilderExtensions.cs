using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Persistence;

/// <summary>
/// Provides shared Entity Framework Core mappings for inbox and outbox messages.
/// </summary>
public static class InboxOutboxModelBuilderExtensions
{
    /// <summary>
    /// Configures the inbox and outbox message entities with consistent table, key, index, and column mappings.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
    public static void ConfigureInboxOutboxMessages(this ModelBuilder modelBuilder)
    {
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
