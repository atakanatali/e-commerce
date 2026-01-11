using Orchestrator.Api.Application.Abstractions;
using Orchestrator.Api.Domain;
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

    /// <inheritdoc />
    public void Add(OutboxMessage message)
    {
        _dbContext.OutboxMessages.Add(message);
    }
}
