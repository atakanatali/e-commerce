using ECommerce.Shared.Contracts.Events;
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

    /// <inheritdoc />
    public async Task<IStockTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new StockTransaction(transaction);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StockReservation>> GetReservationsAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.StockReservations
            .Where(reservation => reservation.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> TryReserveAsync(
        IReadOnlyCollection<OrderItemDto> items,
        DateTime now,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE stock_items SET available_qty = available_qty - {item.Quantity}, reserved_qty = reserved_qty + {item.Quantity}, updated_at_utc = {now}, version = version + 1 WHERE product_id = {item.ProductId} AND available_qty >= {item.Quantity}",
                cancellationToken);

            if (affectedRows == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public void AddReservation(StockReservation reservation)
    {
        _dbContext.StockReservations.Add(reservation);
    }

    /// <inheritdoc />
    public Task<StockItem?> GetStockItemAsync(Guid productId, CancellationToken cancellationToken)
    {
        return _dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.ProductId == productId, cancellationToken);
    }

    /// <inheritdoc />
    public void AddOutboxMessage(OutboxMessage message)
    {
        _dbContext.OutboxMessages.Add(message);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class StockTransaction : IStockTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public StockTransaction(IDbContextTransaction transaction)
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
