using ECommerce.Shared.Messaging;

namespace Stock.Api.Application;

/// <summary>
/// Defines a message handler contract.
/// </summary>
/// <typeparam name="TMessage">The message payload type.</typeparam>
public interface IMessageHandler<TMessage>
{
    /// <summary>
    /// Handles the specified message.
    /// </summary>
    /// <param name="message">The message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(MessageEnvelope<TMessage> message, CancellationToken cancellationToken);
}
