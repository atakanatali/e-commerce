namespace Cloudify.Infrastructure;

/// <summary>
/// Represents the result of a port allocation request.
/// </summary>
public sealed class PortAllocationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PortAllocationResult"/> class.
    /// </summary>
    /// <param name="environmentName">The environment identifier.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="ports">The allocated ports.</param>
    public PortAllocationResult(string environmentName, string resourceId, ServiceType serviceType, IReadOnlyList<int> ports)
    {
        EnvironmentName = environmentName;
        ResourceId = resourceId;
        ServiceType = serviceType;
        Ports = ports;
    }

    /// <summary>
    /// Gets the environment identifier.
    /// </summary>
    public string EnvironmentName { get; }

    /// <summary>
    /// Gets the resource identifier.
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Gets the service type.
    /// </summary>
    public ServiceType ServiceType { get; }

    /// <summary>
    /// Gets the allocated ports.
    /// </summary>
    public IReadOnlyList<int> Ports { get; }
}
