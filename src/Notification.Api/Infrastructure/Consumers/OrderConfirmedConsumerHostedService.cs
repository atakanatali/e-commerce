using System.Text.Json;
using ECommerce.Shared.Contracts.Events;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Messaging.Topology;
using Microsoft.EntityFrameworkCore;
using Notification.Api.Application;
using Notification.Api.Infrastructure.Inbox;
using Notification.Api.Infrastructure.Messaging;
using Notification.Api.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Api.Infrastructure.Consumers;

/// <summary>
/// Consumes order confirmed events for the Notification service.
/// </summary>
public sealed class OrderConfirmedConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRabbitMqConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderConfirmedConsumerHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="connectionFactory">The connection factory.</param>
    public OrderConfirmedConsumerHostedService(
        IServiceProvider serviceProvider,
        IRabbitMqConnectionFactory connectionFactory)
    {
        _serviceProvider = serviceProvider;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Starts consuming order confirmed events from RabbitMQ.
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

        channel.BasicConsume(TopologyConstants.NotificationQueues.OrderConfirmedQueue, autoAck: false, consumer: consumer);
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

        if (messageType == nameof(OrderConfirmedEvent))
        {
            await ProcessAsync<OrderConfirmedEvent, OrderConfirmedEventHandler>(channel, args, body, cancellationToken);
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
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

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
                Consumer = "notification-service",
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
