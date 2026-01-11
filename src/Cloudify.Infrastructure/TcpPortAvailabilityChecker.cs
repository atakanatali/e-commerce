using System.Net;
using System.Net.Sockets;

namespace Cloudify.Infrastructure;

/// <summary>
/// Checks port availability by attempting to bind a TCP listener to localhost.
/// </summary>
public sealed class TcpPortAvailabilityChecker : IPortAvailabilityChecker
{
    /// <summary>
    /// Determines whether a port is available for binding on localhost.
    /// </summary>
    /// <param name="port">The port to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the port is available; otherwise, <c>false</c>.</returns>
    public Task<bool> IsPortAvailableAsync(int port, CancellationToken cancellationToken = default)
    {
        if (port < 1 || port > 65535)
        {
            return Task.FromResult(false);
        }

        TcpListener? listener = null;
        try
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            return Task.FromResult(true);
        }
        catch (SocketException)
        {
            return Task.FromResult(false);
        }
        finally
        {
            listener?.Stop();
        }
    }
}
