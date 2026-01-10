using ECommerce.Shared.Messaging.Topology;
using RabbitMQ.Client;

namespace Order.Api.Infrastructure.Messaging;

/// <summary>
/// Initializes RabbitMQ topology for the Order service.
/// </summary>
public sealed class OrderTopologyInitializer : ITopologyInitializer
{
    private readonly IRabbitMqConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTopologyInitializer"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    public OrderTopologyInitializer(IRabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Declares exchanges, queues, and bindings for the Order service.
    /// </summary>
    public void Initialize()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(TopologyConstants.CommandsExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.ExchangeDeclare(TopologyConstants.EventsExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

        DeclareQueues(channel, TopologyConstants.OrderQueues.StockEventsQueue);

        channel.QueueBind(
            TopologyConstants.OrderQueues.StockEventsQueue,
            TopologyConstants.EventsExchangeName,
            TopologyConstants.EventRoutingKeys.StockReserved);

        channel.QueueBind(
            TopologyConstants.OrderQueues.StockEventsQueue,
            TopologyConstants.EventsExchangeName,
            TopologyConstants.EventRoutingKeys.StockReserveFailed);
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
