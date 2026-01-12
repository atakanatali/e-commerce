using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Stock.Worker.Infrastructure.Persistence;

namespace Stock.Worker.Migrations;

/// <inheritdoc />
[DbContext(typeof(StockDbContext))]
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

        migrationBuilder.CreateTable(
            name: "stock_items",
            columns: table => new
            {
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                AvailableQty = table.Column<int>(type: "integer", nullable: false),
                ReservedQty = table.Column<int>(type: "integer", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_stock_items", x => x.ProductId));

        migrationBuilder.CreateTable(
            name: "stock_reservations",
            columns: table => new
            {
                ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_stock_reservations", x => x.ReservationId));

        migrationBuilder.CreateIndex(
            name: "idx_inbox_status",
            table: "inbox_messages",
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

        migrationBuilder.CreateIndex(
            name: "IX_stock_reservations_OrderId",
            table: "stock_reservations",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_stock_reservations_Status",
            table: "stock_reservations",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "inbox_messages");

        migrationBuilder.DropTable(
            name: "outbox_messages");

        migrationBuilder.DropTable(
            name: "stock_items");

        migrationBuilder.DropTable(
            name: "stock_reservations");
    }
}
