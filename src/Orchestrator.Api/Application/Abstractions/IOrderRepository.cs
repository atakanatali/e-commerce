using Orchestrator.Api.Domain;

namespace Orchestrator.Api.Application.Abstractions;

/// <summary>
/// Provides access to order aggregates.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Adds a new order aggregate.
    /// </summary>
    /// <param name="order">The order aggregate.</param>
    void Add(Order order);

    /// <summary>
    /// Retrieves an order aggregate by its identifier.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order aggregate or null.</returns>
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists pending changes to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
