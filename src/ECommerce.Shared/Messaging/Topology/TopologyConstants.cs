namespace ECommerce.Shared.Messaging.Topology;

/// <summary>
/// Provides standardized exchange, routing key, and queue names for the platform.
/// </summary>
public static class TopologyConstants
{
    /// <summary>
    /// Gets the commands exchange name.
    /// </summary>
    public const string CommandsExchangeName = "ecommerce.commands";

    /// <summary>
    /// Gets the events exchange name.
    /// </summary>
    public const string EventsExchangeName = "ecommerce.events";

    /// <summary>
    /// Provides routing keys for commands.
    /// </summary>
    public static class CommandRoutingKeys
    {
        /// <summary>
        /// Gets the routing key for reserve stock commands.
        /// </summary>
        public const string StockReserve = "stock.reserve";

        /// <summary>
        /// Gets the routing key for release stock commands.
        /// </summary>
        public const string StockRelease = "stock.release";

        /// <summary>
        /// Gets the routing key for send email commands.
        /// </summary>
        public const string NotificationEmailSend = "notification.email.send";

        /// <summary>
        /// Gets the routing key for send SMS commands.
        /// </summary>
        public const string NotificationSmsSend = "notification.sms.send";
    }

    /// <summary>
    /// Provides routing keys for events.
    /// </summary>
    public static class EventRoutingKeys
    {
        /// <summary>
        /// Gets the routing key for order created events.
        /// </summary>
        public const string OrderCreated = "order.created";

        /// <summary>
        /// Gets the routing key for order confirmed events.
        /// </summary>
        public const string OrderConfirmed = "order.confirmed";

        /// <summary>
        /// Gets the routing key for order cancelled events.
        /// </summary>
        public const string OrderCancelled = "order.cancelled";

        /// <summary>
        /// Gets the routing key for stock reserved events.
        /// </summary>
        public const string StockReserved = "stock.reserved";

        /// <summary>
        /// Gets the routing key for stock reservation failed events.
        /// </summary>
        public const string StockReserveFailed = "stock.reserve_failed";

        /// <summary>
        /// Gets the routing key for notification sent events.
        /// </summary>
        public const string NotificationSent = "notification.sent";

        /// <summary>
        /// Gets the routing key for notification failed events.
        /// </summary>
        public const string NotificationFailed = "notification.failed";
    }

    /// <summary>
    /// Provides queue names for order service consumers.
    /// </summary>
    public static class OrderQueues
    {
        /// <summary>
        /// Gets the stock events queue name.
        /// </summary>
        public const string StockEventsQueue = "order-service.stock-events-queue";
    }

    /// <summary>
    /// Provides queue names for stock service consumers.
    /// </summary>
    public static class StockQueues
    {
        /// <summary>
        /// Gets the order events queue name.
        /// </summary>
        public const string OrderEventsQueue = "stock-service.order-events-queue";
    }

    /// <summary>
    /// Provides queue names for notification service consumers.
    /// </summary>
    public static class NotificationQueues
    {
        /// <summary>
        /// Gets the order confirmed queue name.
        /// </summary>
        public const string OrderConfirmedQueue = "notification-service.order-confirmed-queue";
    }
}
