using RabbitMQ.Client;

namespace Stock.Api.Infrastructure.Messaging;

/// <summary>
/// Provides RabbitMQ connections for the service.
/// </summary>
public interface IRabbitMqConnectionFactory
{
    /// <summary>
    /// Creates a new RabbitMQ connection.
    /// </summary>
    /// <returns>The connection instance.</returns>
    IConnection CreateConnection();
}
