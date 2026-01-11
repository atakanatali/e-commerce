using ECommerce.Shared.Messaging.Topology;
using ECommerce.Messaging.RabbitMq;
using RabbitMQ.Client;

namespace Notification.Worker.Infrastructure.Messaging;

/// <summary>
/// Initializes RabbitMQ topology for the Notification service.
/// </summary>
public sealed class NotificationTopologyInitializer : ITopologyInitializer
{
    private readonly IRabbitMqConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationTopologyInitializer"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    public NotificationTopologyInitializer(IRabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Declares exchanges, queues, and bindings for the Notification service.
    /// </summary>
    public void Initialize()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(TopologyConstants.CommandsExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.ExchangeDeclare(TopologyConstants.EventsExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

        DeclareQueues(channel, TopologyConstants.NotificationQueues.OrderConfirmedQueue);

        channel.QueueBind(
            TopologyConstants.NotificationQueues.OrderConfirmedQueue,
            TopologyConstants.EventsExchangeName,
            TopologyConstants.EventRoutingKeys.OrderConfirmed);
    }

    /// <summary>
    /// Declares the main, retry, and dead-letter queues for a consumer.
    /// </summary>
    /// <param name="channel">The RabbitMQ channel.</param>
    /// <param name="queueName">The base queue name.</param>
    private static void DeclareQueues(IModel channel, string queueName)
    {
        var retryQueue = $"{queueName}.retry";
        var dlqQueue = $"{queueName}.dlq";

        channel.QueueDeclare(dlqQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
        {
            ["x-message-ttl"] = 30000,
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = queueName
        });

        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = retryQueue
        });
    }
}
