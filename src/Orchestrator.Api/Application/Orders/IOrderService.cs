using Orchestrator.Api.Contracts;

namespace Orchestrator.Api.Application.Orders;

/// <summary>
/// Coordinates order-related application workflows.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order and emits an outbox message.
    /// </summary>
    /// <param name="request">The create order request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The create order response.</returns>
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}
