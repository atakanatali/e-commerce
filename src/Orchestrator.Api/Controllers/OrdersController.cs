using ECommerce.Core.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Api.Application.Orders;
using Orchestrator.Api.Contracts;

namespace Orchestrator.Api.Controllers;

/// <summary>
/// Provides endpoints for order operations.
/// </summary>
[ApiController]
[Route("orders")]
[Throttling(
    key: RateLimitKeyType.Ip | RateLimitKeyType.Path,
    duration: 1,
    type: RateLimitTimeUnit.Minute,
    limit: 100,
    limitType: RateLimitTimeUnit.Minute)]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="orderService">The order service.</param>
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order and publishes an order created event.
    /// </summary>
    /// <param name="request">The create order request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created order response.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0 || request.Items.Any(item => item.Quantity <= 0))
        {
            return BadRequest("Order items are invalid.");
        }

        var response = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(CreateOrder), response);
    }
}
