namespace Cloudify.Infrastructure;

/// <summary>
/// Defines storage operations for environment port allocations.
/// </summary>
public interface IStateStore
{
    /// <summary>
    /// Updates port allocations for an environment in a concurrency-safe manner.
    /// </summary>
    /// <param name="environmentName">The environment identifier.</param>
    /// <param name="update">The update function applied to current state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated <see cref="ResourcePorts"/> state.</returns>
    Task<ResourcePorts> UpdateResourcePortsAsync(
        string environmentName,
        Func<ResourcePorts?, CancellationToken, Task<ResourcePorts>> update,
        CancellationToken cancellationToken = default);
}
