using ECommerce.Shared.Contracts.Events;
using Stock.Worker.Domain;

namespace Stock.Worker.Application.Abstractions;

/// <summary>
/// Provides access to stock aggregates and outbox storage.
/// </summary>
public interface IStockRepository
{
    /// <summary>
    /// Begins a transaction for stock operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction handle.</returns>
    Task<IStockTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves existing reservations for an order.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of reservations.</returns>
    Task<IReadOnlyList<StockReservation>> GetReservationsAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to reserve stock for each item.
    /// </summary>
    /// <param name="items">The order items.</param>
    /// <param name="now">The current timestamp.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when reservation succeeds.</returns>
    Task<bool> TryReserveAsync(IReadOnlyCollection<OrderItemDto> items, DateTime now, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new stock reservation.
    /// </summary>
    /// <param name="reservation">The reservation.</param>
    void AddReservation(StockReservation reservation);

    /// <summary>
    /// Gets a stock item by product identifier.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stock item if found.</returns>
    Task<StockItem?> GetStockItemAsync(Guid productId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an outbox message.
    /// </summary>
    /// <param name="message">The outbox message.</param>
    void AddOutboxMessage(OutboxMessage message);

    /// <summary>
    /// Persists pending changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
