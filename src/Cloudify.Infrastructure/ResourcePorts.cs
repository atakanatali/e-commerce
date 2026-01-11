namespace Cloudify.Infrastructure;

/// <summary>
/// Stores port allocations for resources within an environment.
/// </summary>
public sealed class ResourcePorts
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePorts"/> class.
    /// </summary>
    /// <param name="environmentName">The environment identifier.</param>
    public ResourcePorts(string environmentName)
    {
        EnvironmentName = environmentName;
        Assignments = new Dictionary<string, PortAssignment>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the environment identifier.
    /// </summary>
    public string EnvironmentName { get; }

    /// <summary>
    /// Gets the assignments keyed by resource id.
    /// </summary>
    public Dictionary<string, PortAssignment> Assignments { get; }

    /// <summary>
    /// Gets all allocated ports in the environment.
    /// </summary>
    /// <returns>The set of allocated ports.</returns>
    public HashSet<int> GetAllPorts()
    {
        var ports = new HashSet<int>();
        foreach (var assignment in Assignments.Values)
        {
            foreach (var port in assignment.Ports)
            {
                ports.Add(port);
            }
        }

        return ports;
    }
}
