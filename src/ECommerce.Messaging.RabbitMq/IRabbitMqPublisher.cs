namespace ECommerce.Messaging.RabbitMq;

using ECommerce.Shared.Messaging;

namespace ECommerce.Messaging.RabbitMq;

/// <summary>
/// Publishes message envelopes to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publishes a message envelope to the specified exchange.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="envelope">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task PublishAsync<TPayload>(
        string exchange,
        string routingKey,
        MessageEnvelope<TPayload> envelope,
        CancellationToken cancellationToken);
}
