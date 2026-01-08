using System.Text.Json;
using ECommerce.Shared.Messaging;
using RabbitMQ.Client;

namespace StockService.Infrastructure.Messaging;

/// <summary>
/// Publishes message envelopes to RabbitMQ.
/// </summary>
public sealed class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IRabbitMqConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqPublisher"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory.</param>
    public RabbitMqPublisher(IRabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Publishes a message envelope to the broker.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="envelope">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    public Task PublishAsync<TPayload>(
        string exchange,
        string routingKey,
        MessageEnvelope<TPayload> envelope,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        var body = JsonSerializer.SerializeToUtf8Bytes(envelope);
        var properties = channel.CreateBasicProperties();
        properties.MessageId = envelope.MessageId.ToString();
        properties.Type = envelope.MessageType;
        properties.CorrelationId = envelope.CorrelationId.ToString();
        properties.Headers = new Dictionary<string, object>
        {
            ["x-causation-id"] = envelope.CausationId?.ToString() ?? string.Empty,
            ["x-occurred-at-utc"] = envelope.OccurredAtUtc.ToString("O"),
            ["x-producer"] = envelope.Producer,
            ["x-version"] = envelope.Version
        };

        channel.BasicPublish(exchange, routingKey, mandatory: true, basicProperties: properties, body: body);
        return Task.CompletedTask;
    }
}
