namespace Cloudify.Infrastructure;

/// <summary>
/// Describes a port allocation request for a resource in an environment.
/// </summary>
public sealed class PortAllocationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PortAllocationRequest"/> class.
    /// </summary>
    /// <param name="environmentName">The environment identifier for allocation.</param>
    /// <param name="resourceId">The resource identifier within the environment.</param>
    /// <param name="serviceType">The service type requiring ports.</param>
    public PortAllocationRequest(string environmentName, string resourceId, ServiceType serviceType)
    {
        EnvironmentName = environmentName;
        ResourceId = resourceId;
        ServiceType = serviceType;
    }

    /// <summary>
    /// Gets the environment identifier for allocation.
    /// </summary>
    public string EnvironmentName { get; }

    /// <summary>
    /// Gets the resource identifier within the environment.
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Gets the service type requiring ports.
    /// </summary>
    public ServiceType ServiceType { get; }

    /// <summary>
    /// Gets or sets the explicitly requested primary port.
    /// </summary>
    public int? RequestedPort { get; set; }
}
