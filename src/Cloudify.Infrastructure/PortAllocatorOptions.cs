namespace Cloudify.Infrastructure;

/// <summary>
/// Provides configuration for port allocation behavior.
/// </summary>
public sealed class PortAllocatorOptions
{
    /// <summary>
    /// Gets or sets the ordered base port options for application services.
    /// </summary>
    public IReadOnlyList<int> AppServiceBasePorts { get; set; } = new List<int> { 8080, 5000 };
}
