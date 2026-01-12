using ECommerce.Shared.Messaging;
using Orchestrator.Api.Application.Abstractions;
using Orchestrator.Api.Infrastructure.Persistence;

namespace Orchestrator.Api.Infrastructure.Outbox;

/// <summary>
/// Provides EF Core access for outbox messages.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OutboxRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds an outbox message to the database context for later persistence.
    /// </summary>
    /// <param name="message">The outbox message to enqueue.</param>
    public void Add(OutboxMessage message)
    {
        _dbContext.OutboxMessages.Add(message);
    }
}
