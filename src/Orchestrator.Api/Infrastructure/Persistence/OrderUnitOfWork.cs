using Microsoft.EntityFrameworkCore.Storage;
using Orchestrator.Api.Application.Abstractions;

namespace Orchestrator.Api.Infrastructure.Persistence;

/// <summary>
/// Provides transactional coordination for order persistence.
/// </summary>
public sealed class OrderUnitOfWork : IOrderUnitOfWork
{
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderUnitOfWork"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OrderUnitOfWork(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new OrderTransaction(transaction);
    }

    private sealed class OrderTransaction : IOrderTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public OrderTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return _transaction.RollbackAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }
    }
}
