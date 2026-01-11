namespace Cloudify.Infrastructure;

/// <summary>
/// Represents an allocated port assignment for a resource.
/// </summary>
public sealed class PortAssignment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PortAssignment"/> class.
    /// </summary>
    /// <param name="serviceType">The service type associated with the ports.</param>
    /// <param name="ports">The allocated ports for the resource.</param>
    public PortAssignment(ServiceType serviceType, IReadOnlyList<int> ports)
    {
        ServiceType = serviceType;
        Ports = ports;
    }

    /// <summary>
    /// Gets the service type associated with the ports.
    /// </summary>
    public ServiceType ServiceType { get; }

    /// <summary>
    /// Gets the allocated ports for the resource.
    /// </summary>
    public IReadOnlyList<int> Ports { get; }
}
