namespace ECommerce.Core.Redis;

/// <summary>
/// Represents configuration settings for Redis connectivity.
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Redis connectivity is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the deployment mode for Redis.
    /// </summary>
    public RedisMode Mode { get; set; } = RedisMode.Single;

    /// <summary>
    /// Gets or sets the connection string for single-instance Redis deployments.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the endpoints for clustered Redis deployments.
    /// </summary>
    public string[] Endpoints { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the optional Redis password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SSL should be used.
    /// </summary>
    public bool Ssl { get; set; }

    /// <summary>
    /// Gets or sets the default database index.
    /// </summary>
    public int? DefaultDatabase { get; set; }

    /// <summary>
    /// Gets or sets the instance name or client name prefix.
    /// </summary>
    public string? InstanceName { get; set; }
}
