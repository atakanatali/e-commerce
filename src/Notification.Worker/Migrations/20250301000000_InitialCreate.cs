using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Notification.Worker.Infrastructure.Persistence;

namespace Notification.Worker.Migrations;

/// <inheritdoc />
[DbContext(typeof(NotificationDbContext))]
[Migration("20250301000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inbox_messages",
            columns: table => new
            {
                MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                MessageType = table.Column<string>(type: "text", nullable: false),
                CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                Consumer = table.Column<string>(type: "text", nullable: false),
                Handler = table.Column<string>(type: "text", nullable: false),
                ReceivedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                ProcessedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                LastError = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_inbox_messages", x => x.MessageId));

        migrationBuilder.CreateTable(
            name: "notification_logs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                Channel = table.Column<string>(type: "text", nullable: false),
                Recipient = table.Column<string>(type: "text", nullable: false),
                Template = table.Column<string>(type: "text", nullable: false),
                VariablesJson = table.Column<string>(type: "jsonb", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                Attempt = table.Column<int>(type: "integer", nullable: false),
                ProviderMessageId = table.Column<string>(type: "text", nullable: true),
                ProviderResponseJson = table.Column<string>(type: "jsonb", nullable: true),
                LastError = table.Column<string>(type: "text", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_notification_logs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                MessageType = table.Column<string>(type: "text", nullable: false),
                Exchange = table.Column<string>(type: "text", nullable: false),
                RoutingKey = table.Column<string>(type: "text", nullable: false),
                CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                CausationId = table.Column<Guid>(type: "uuid", nullable: true),
                OccurredAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                Producer = table.Column<string>(type: "text", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false),
                PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                ProcessedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                RetryCount = table.Column<int>(type: "integer", nullable: false),
                LastError = table.Column<string>(type: "text", nullable: true),
                LockedUntilUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                LockedBy = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_outbox_messages", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "idx_inbox_status",
            table: "inbox_messages",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_notification_logs_OrderId",
            table: "notification_logs",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_notification_logs_Status",
            table: "notification_logs",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_outbox_messages_MessageId",
            table: "outbox_messages",
            column: "MessageId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "idx_outbox_unprocessed",
            table: "outbox_messages",
            column: "ProcessedAtUtc");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "inbox_messages");

        migrationBuilder.DropTable(
            name: "notification_logs");

        migrationBuilder.DropTable(
            name: "outbox_messages");
    }
}
