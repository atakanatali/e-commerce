using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockService.Application;
using StockService.Infrastructure.Inbox;
using StockService.Infrastructure.Messaging;
using StockService.Infrastructure.Persistence;

namespace StockService.Infrastructure.Consumers;

/// <summary>
/// Consumes order-related events for the Stock service.
/// </summary>
public sealed class OrderEventsConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMqConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderEventsConsumerHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="connectionFactory">The connection factory.</param>
    public OrderEventsConsumerHostedService(
        IServiceProvider serviceProvider,
        IRabbitMqConnectionFactory connectionFactory)
    {
        _serviceProvider = serviceProvider;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Starts consuming order events from RabbitMQ.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = _connectionFactory.CreateConnection();
        var channel = connection.CreateModel();
        channel.BasicQos(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, args) =>
        {
            await HandleMessageAsync(channel, args, stoppingToken);
        };

        channel.BasicConsume(TopologyConstants.StockQueues.OrderEventsQueue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles a delivery from RabbitMQ and dispatches to the correct handler.
    /// </summary>
    /// <param name="channel">The RabbitMQ channel.</param>
    /// <param name="args">The delivery arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleMessageAsync(IModel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        var messageType = args.BasicProperties.Type;
        var body = args.Body.ToArray();

        if (messageType == nameof(OrderCreatedEvent))
        {
            await ProcessAsync<OrderCreatedEvent, OrderCreatedEventHandler>(channel, args, body, cancellationToken);
            return;
        }

        channel.BasicAck(args.DeliveryTag, false);
    }

    /// <summary>
    /// Processes an envelope with inbox idempotency and a typed handler.
    /// </summary>
    /// <typeparam name="TMessage">The message payload type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="channel">The RabbitMQ channel.</param>
    /// <param name="args">The delivery arguments.</param>
    /// <param name="body">The message body bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ProcessAsync<TMessage, THandler>(
        IModel channel,
        BasicDeliverEventArgs args,
        byte[] body,
        CancellationToken cancellationToken)
        where THandler : IMessageHandler<TMessage>
    {
        var envelope = JsonSerializer.Deserialize<MessageEnvelope<TMessage>>(body);

        if (envelope is null)
        {
            channel.BasicAck(args.DeliveryTag, false);
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockDbContext>();

        var inbox = await dbContext.InboxMessages
            .FirstOrDefaultAsync(message => message.MessageId == envelope.MessageId, cancellationToken);

        if (inbox?.Status == "Processed")
        {
            channel.BasicAck(args.DeliveryTag, false);
            return;
        }

        if (inbox is null)
        {
            inbox = new InboxMessage
            {
                MessageId = envelope.MessageId,
                MessageType = envelope.MessageType,
                CorrelationId = envelope.CorrelationId,
                Consumer = "stock-service",
                Handler = typeof(THandler).Name,
                ReceivedAtUtc = DateTime.UtcNow,
                Status = "Received"
            };

            dbContext.InboxMessages.Add(inbox);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        try
        {
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            await handler.HandleAsync(envelope, cancellationToken);

            inbox.Status = "Processed";
            inbox.ProcessedAtUtc = DateTime.UtcNow;
            inbox.LastError = null;
            await dbContext.SaveChangesAsync(cancellationToken);

            channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            inbox.Status = "Failed";
            inbox.LastError = ex.Message;
            await dbContext.SaveChangesAsync(cancellationToken);

            channel.BasicNack(args.DeliveryTag, false, requeue: false);
        }
    }
}
