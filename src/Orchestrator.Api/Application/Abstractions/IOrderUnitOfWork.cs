namespace Orchestrator.Api.Application.Abstractions;

/// <summary>
/// Provides transactional coordination for order persistence.
/// </summary>
public interface IOrderUnitOfWork
{
    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction handle.</returns>
    Task<IOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
