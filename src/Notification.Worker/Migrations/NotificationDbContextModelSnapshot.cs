using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Notification.Worker.Infrastructure.Persistence;

namespace Notification.Worker.Migrations;

/// <inheritdoc />
[DbContext(typeof(NotificationDbContext))]
public partial class NotificationDbContextModelSnapshot : ModelSnapshot
{
    /// <inheritdoc />
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity("ECommerce.Shared.Messaging.InboxMessage", b =>
        {
            b.Property<Guid>("MessageId")
                .HasColumnType("uuid");

            b.Property<string>("Consumer")
                .HasColumnType("text");

            b.Property<Guid>("CorrelationId")
                .HasColumnType("uuid");

            b.Property<string>("Handler")
                .HasColumnType("text");

            b.Property<string>("LastError")
                .HasColumnType("text");

            b.Property<string>("MessageType")
                .HasColumnType("text");

            b.Property<DateTime?>("ProcessedAtUtc")
                .HasColumnType("timestamptz");

            b.Property<DateTime>("ReceivedAtUtc")
                .HasColumnType("timestamptz");

            b.Property<string>("Status")
                .HasColumnType("text");

            b.HasKey("MessageId");

            b.HasIndex("Status")
                .HasDatabaseName("idx_inbox_status");

            b.ToTable("inbox_messages");
        });

        modelBuilder.Entity("ECommerce.Shared.Messaging.OutboxMessage", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<Guid?>("CausationId")
                .HasColumnType("uuid");

            b.Property<Guid>("CorrelationId")
                .HasColumnType("uuid");

            b.Property<string>("Exchange")
                .HasColumnType("text");

            b.Property<string>("LastError")
                .HasColumnType("text");

            b.Property<string>("LockedBy")
                .HasColumnType("text");

            b.Property<DateTime?>("LockedUntilUtc")
                .HasColumnType("timestamptz");

            b.Property<Guid>("MessageId")
                .HasColumnType("uuid");

            b.Property<string>("MessageType")
                .HasColumnType("text");

            b.Property<DateTime>("OccurredAtUtc")
                .HasColumnType("timestamptz");

            b.Property<string>("PayloadJson")
                .HasColumnType("jsonb");

            b.Property<DateTime?>("ProcessedAtUtc")
                .HasColumnType("timestamptz");

            b.Property<string>("Producer")
                .HasColumnType("text");

            b.Property<int>("RetryCount")
                .HasColumnType("integer");

            b.Property<string>("RoutingKey")
                .HasColumnType("text");

            b.Property<int>("Version")
                .HasColumnType("integer");

            b.HasKey("Id");

            b.HasIndex("MessageId")
                .IsUnique();

            b.HasIndex("ProcessedAtUtc")
                .HasDatabaseName("idx_outbox_unprocessed");

            b.ToTable("outbox_messages");
        });

        modelBuilder.Entity("Notification.Worker.Domain.NotificationLog", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<int>("Attempt")
                .HasColumnType("integer");

            b.Property<string>("Channel")
                .HasColumnType("text");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamptz");

            b.Property<string>("LastError")
                .HasColumnType("text");

            b.Property<Guid>("OrderId")
                .HasColumnType("uuid");

            b.Property<string?>("ProviderMessageId")
                .HasColumnType("text");

            b.Property<string?>("ProviderResponseJson")
                .HasColumnType("jsonb");

            b.Property<string>("Recipient")
                .HasColumnType("text");

            b.Property<string>("Status")
                .HasColumnType("text");

            b.Property<string>("Template")
                .HasColumnType("text");

            b.Property<DateTime>("UpdatedAtUtc")
                .HasColumnType("timestamptz");

            b.Property<string>("VariablesJson")
                .HasColumnType("jsonb");

            b.HasKey("Id");

            b.HasIndex("OrderId");

            b.HasIndex("Status");

            b.ToTable("notification_logs");
        });
    }
}
