using Orchestrator.Api.Domain;

namespace Orchestrator.Api.Application.Abstractions;

/// <summary>
/// Provides access to outbox messages.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a new outbox message.
    /// </summary>
    /// <param name="message">The outbox message.</param>
    void Add(OutboxMessage message);
}
