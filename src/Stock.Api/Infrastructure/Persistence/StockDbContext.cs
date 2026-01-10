using Microsoft.EntityFrameworkCore;
using Stock.Api.Domain;
using Stock.Api.Infrastructure.Inbox;
using Stock.Api.Infrastructure.Outbox;

namespace Stock.Api.Infrastructure.Persistence;

/// <summary>
/// Provides the Entity Framework Core database context for the Stock service.
/// </summary>
public sealed class StockDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StockDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the stock items set.
    /// </summary>
    public DbSet<StockItem> StockItems => Set<StockItem>();

    /// <summary>
    /// Gets the stock reservations set.
    /// </summary>
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

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
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.ToTable("stock_items");
            entity.HasKey(item => item.ProductId);
            entity.Property(item => item.UpdatedAtUtc).HasColumnType("timestamptz");
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.ToTable("stock_reservations");
            entity.HasKey(reservation => reservation.ReservationId);
            entity.Property(reservation => reservation.ExpiresAtUtc).HasColumnType("timestamptz");
            entity.Property(reservation => reservation.CreatedAtUtc).HasColumnType("timestamptz");
            entity.Property(reservation => reservation.UpdatedAtUtc).HasColumnType("timestamptz");
            entity.HasIndex(reservation => reservation.OrderId);
            entity.HasIndex(reservation => reservation.Status);
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
