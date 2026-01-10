using ECommerce.Shared.Messaging;

namespace Stock.Api.Infrastructure.Messaging;

/// <summary>
/// Publishes messages to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publishes a message envelope to the broker.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="envelope">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishAsync<TPayload>(
        string exchange,
        string routingKey,
        MessageEnvelope<TPayload> envelope,
        CancellationToken cancellationToken);
}
