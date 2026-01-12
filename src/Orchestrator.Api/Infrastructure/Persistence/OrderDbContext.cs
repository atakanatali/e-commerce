using ECommerce.Core.Persistence;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Api.Domain;

namespace Orchestrator.Api.Infrastructure.Persistence;

/// <summary>
/// Provides the Entity Framework Core database context for the Orchestrator service.
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
    /// Configures entity mappings for orders, order items, and shared inbox/outbox tables.
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

        modelBuilder.ConfigureInboxOutboxMessages();
    }
}
