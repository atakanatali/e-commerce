using RabbitMQ.Client;

namespace ECommerce.Messaging.RabbitMq;

/// <summary>
/// Creates RabbitMQ connections.
/// </summary>
public interface IRabbitMqConnectionFactory
{
    /// <summary>
    /// Creates a new RabbitMQ connection.
    /// </summary>
    /// <returns>The connection.</returns>
    IConnection CreateConnection();
}
