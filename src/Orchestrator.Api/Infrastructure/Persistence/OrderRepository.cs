using Microsoft.EntityFrameworkCore;
using Orchestrator.Api.Application.Abstractions;
using Orchestrator.Api.Domain;

namespace Orchestrator.Api.Infrastructure.Persistence;

/// <summary>
/// Provides EF Core access for order aggregates.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public void Add(Order order)
    {
        _dbContext.Orders.Add(order);
    }

    /// <inheritdoc />
    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return _dbContext.Orders.FirstOrDefaultAsync(entity => entity.Id == orderId, cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
