namespace Cloudify.Infrastructure;

/// <summary>
/// Defines port allocation operations for services.
/// </summary>
public interface IPortAllocator
{
    /// <summary>
    /// Allocates ports for a resource based on allocation policies.
    /// </summary>
    /// <param name="request">The allocation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocated port result.</returns>
    Task<PortAllocationResult> AllocateAsync(PortAllocationRequest request, CancellationToken cancellationToken = default);
}
