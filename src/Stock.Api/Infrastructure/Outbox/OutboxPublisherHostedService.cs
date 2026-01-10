using System.Text.Json;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stock.Api.Infrastructure.Messaging;
using Stock.Api.Infrastructure.Persistence;

namespace Stock.Api.Infrastructure.Outbox;

/// <summary>
/// Publishes pending outbox messages to RabbitMQ.
/// </summary>
public sealed class OutboxPublisherHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMqPublisher _publisher;
    private readonly RabbitMqOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxPublisherHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="publisher">The RabbitMQ publisher.</param>
    /// <param name="options">The RabbitMQ options.</param>
    public OutboxPublisherHostedService(
        IServiceProvider serviceProvider,
        IRabbitMqPublisher publisher,
        IOptions<RabbitMqOptions> options)
    {
        _serviceProvider = serviceProvider;
        _publisher = publisher;
        _options = options.Value;
    }

    /// <summary>
    /// Executes the background publishing loop.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishBatchAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    /// <summary>
    /// Publishes a batch of unprocessed outbox messages.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task PublishBatchAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockDbContext>();

        var now = DateTime.UtcNow;
        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null && (message.LockedUntilUtc == null || message.LockedUntilUtc < now))
            .OrderBy(message => message.OccurredAtUtc)
            .Take(50)
            .ToListAsync(stoppingToken);

        foreach (var message in messages)
        {
            message.LockedUntilUtc = now.AddSeconds(60);
            message.LockedBy = _options.ServiceName;
        }

        await dbContext.SaveChangesAsync(stoppingToken);

        foreach (var message in messages)
        {
            try
            {
                var envelopeType = typeof(MessageEnvelope<>).MakeGenericType(typeof(object));
                var envelope = JsonSerializer.Deserialize(message.PayloadJson, envelopeType);

                if (envelope is null)
                {
                    message.RetryCount++;
                    message.LastError = "Envelope deserialization failed.";
                    continue;
                }

                await _publisher.PublishAsync(message.Exchange, message.RoutingKey, (dynamic)envelope, stoppingToken);
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LockedUntilUtc = null;
                message.LockedBy = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.LastError = ex.Message;
                message.LockedUntilUtc = DateTime.UtcNow.AddSeconds(30);
                message.LockedBy = null;
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
