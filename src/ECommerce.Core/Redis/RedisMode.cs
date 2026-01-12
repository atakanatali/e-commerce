namespace ECommerce.Core.Redis;

/// <summary>
/// Represents the Redis deployment mode.
/// </summary>
public enum RedisMode
{
    /// <summary>
    /// A single Redis instance is used.
    /// </summary>
    Single,

    /// <summary>
    /// A Redis cluster deployment is used.
    /// </summary>
    Cluster
}
