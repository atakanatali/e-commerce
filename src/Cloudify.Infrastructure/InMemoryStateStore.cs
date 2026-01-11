using System.Collections.Concurrent;

namespace Cloudify.Infrastructure;

/// <summary>
/// Provides an in-memory implementation of <see cref="IStateStore"/>.
/// </summary>
public sealed class InMemoryStateStore : IStateStore
{
    private readonly ConcurrentDictionary<string, ResourcePorts> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Updates port allocations for an environment in a concurrency-safe manner.
    /// </summary>
    /// <param name="environmentName">The environment identifier.</param>
    /// <param name="update">The update function applied to current state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated <see cref="ResourcePorts"/> state.</returns>
    public async Task<ResourcePorts> UpdateResourcePortsAsync(
        string environmentName,
        Func<ResourcePorts?, CancellationToken, Task<ResourcePorts>> update,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _store.TryGetValue(environmentName, out var existing);
            var updated = await update(existing, cancellationToken).ConfigureAwait(false);
            _store[environmentName] = updated;
            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }
}
