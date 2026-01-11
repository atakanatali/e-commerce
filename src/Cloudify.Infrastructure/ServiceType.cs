namespace Cloudify.Infrastructure;

/// <summary>
/// Defines supported service types for port allocation.
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Represents a Redis service.
    /// </summary>
    Redis,
    /// <summary>
    /// Represents a PostgreSQL service.
    /// </summary>
    Postgres,
    /// <summary>
    /// Represents a MongoDB service.
    /// </summary>
    Mongo,
    /// <summary>
    /// Represents a RabbitMQ service including management port.
    /// </summary>
    RabbitMq,
    /// <summary>
    /// Represents an application service.
    /// </summary>
    AppService
}
