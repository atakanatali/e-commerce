using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Stock.Worker.Application.Abstractions;
using Stock.Worker.Domain;

namespace Stock.Worker.Infrastructure.Persistence;

/// <summary>
/// Provides EF Core access for stock data.
/// </summary>
public sealed class StockRepository : IStockRepository
{
    private readonly StockDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public StockRepository(StockDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Starts a new database transaction that wraps stock operations for consistent writes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to abort the transaction creation.</param>
    /// <returns>A stock transaction wrapper that can be committed or rolled back.</returns>
    public async Task<IStockTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new StockTransaction(transaction);
    }

    /// <summary>
    /// Retrieves all reservations for a specific order so downstream logic can detect existing outcomes.
    /// </summary>
    /// <param name="orderId">The order identifier to search for reservations.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the query.</param>
    /// <returns>The list of reservations for the order.</returns>
    public async Task<IReadOnlyList<StockReservation>> GetReservationsAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.StockReservations
            .Where(reservation => reservation.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Attempts to atomically decrement available stock and increment reserved stock for each item.
    /// </summary>
    /// <param name="items">The order items that need to be reserved.</param>
    /// <param name="now">The current timestamp to stamp stock rows.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the operation.</param>
    /// <returns>True when all items are reserved; otherwise false.</returns>
    public async Task<bool> TryReserveAsync(
        IReadOnlyCollection<OrderItemDto> items,
        DateTime now,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            try
            {
                var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE stock_items SET available_qty = available_qty - {item.Quantity}, reserved_qty = reserved_qty + {item.Quantity}, updated_at_utc = {now}, version = version + 1 WHERE product_id = {item.ProductId} AND available_qty >= {item.Quantity}",
                    cancellationToken);

                if (affectedRows == 0)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return true;
    }

    /// <summary>
    /// Adds a new reservation entity to the change tracker.
    /// </summary>
    /// <param name="reservation">The reservation to persist.</param>
    public void AddReservation(StockReservation reservation)
    {
        _dbContext.StockReservations.Add(reservation);
    }

    /// <summary>
    /// Retrieves the current stock item for a product without tracking changes.
    /// </summary>
    /// <param name="productId">The product identifier to load.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the query.</param>
    /// <returns>The stock item if found; otherwise null.</returns>
    public Task<StockItem?> GetStockItemAsync(Guid productId, CancellationToken cancellationToken)
    {
        return _dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.ProductId == productId, cancellationToken);
    }

    /// <summary>
    /// Adds an outbox message to be published after persistence.
    /// </summary>
    /// <param name="message">The outbox message to store.</param>
    public void AddOutboxMessage(OutboxMessage message)
    {
        _dbContext.OutboxMessages.Add(message);
    }

    /// <summary>
    /// Saves all pending changes in the underlying database context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to stop the save operation.</param>
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class StockTransaction : IStockTransaction
    {
        private readonly IDbContextTransaction _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="StockTransaction"/> class.
        /// </summary>
        /// <param name="transaction">The EF Core transaction to wrap.</param>
        public StockTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// Commits the underlying database transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to stop the commit.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task CommitAsync(CancellationToken cancellationToken)
        {
            return _transaction.CommitAsync(cancellationToken);
        }

        /// <summary>
        /// Rolls back the underlying database transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to stop the rollback.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return _transaction.RollbackAsync(cancellationToken);
        }

        /// <summary>
        /// Disposes the underlying transaction asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            return _transaction.DisposeAsync();
        }
    }
}
