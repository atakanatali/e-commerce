using ECommerce.Core.Persistence;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Stock.Worker.Domain;

namespace Stock.Worker.Infrastructure.Persistence;

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
    /// Configures entity mappings for stock items, stock reservations, and shared inbox/outbox tables.
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

        modelBuilder.ConfigureInboxOutboxMessages();
    }
}
