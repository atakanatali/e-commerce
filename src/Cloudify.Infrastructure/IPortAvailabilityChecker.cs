namespace Cloudify.Infrastructure;

/// <summary>
/// Checks whether a port is available on the local host.
/// </summary>
public interface IPortAvailabilityChecker
{
    /// <summary>
    /// Determines whether a port is available for binding on localhost.
    /// </summary>
    /// <param name="port">The port to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the port is available; otherwise, <c>false</c>.</returns>
    Task<bool> IsPortAvailableAsync(int port, CancellationToken cancellationToken = default);
}
