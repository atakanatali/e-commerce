using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;

namespace Stock.Worker.Application;

/// <summary>
/// Handles order created events by attempting stock reservation.
/// </summary>
public sealed class OrderCreatedEventHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IStockReservationService _reservationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCreatedEventHandler"/> class.
    /// </summary>
    /// <param name="reservationService">The stock reservation service.</param>
    public OrderCreatedEventHandler(IStockReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Handles the specified order created event.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(MessageEnvelope<OrderCreatedEvent> message, CancellationToken cancellationToken)
    {
        await _reservationService.HandleOrderCreatedAsync(message, cancellationToken);
    }
}
