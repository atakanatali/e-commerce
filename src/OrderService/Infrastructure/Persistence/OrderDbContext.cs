using Microsoft.EntityFrameworkCore;
using OrderService.Domain;
using OrderService.Infrastructure.Inbox;
using OrderService.Infrastructure.Outbox;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Provides the Entity Framework Core database context for the Order service.
/// </summary>
public sealed class OrderDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the orders set.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Gets the order items set.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

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
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Status).IsRequired();
            entity.Property(order => order.Total).HasColumnType("numeric(18,2)");
            entity.Property(order => order.CreatedAtUtc).HasColumnType("timestamptz");
            entity.Property(order => order.UpdatedAtUtc).HasColumnType("timestamptz");
            entity.HasIndex(order => order.UserId);
            entity.HasIndex(order => order.Status);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UnitPrice).HasColumnType("numeric(18,2)");
            entity.Property(item => item.LineTotal).HasColumnType("numeric(18,2)");
            entity.HasIndex(item => item.OrderId);
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
