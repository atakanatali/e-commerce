namespace ECommerce.Shared.Contracts.Commands;

/// <summary>
/// Represents a command to send an email notification.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="To">The recipient email address.</param>
/// <param name="Template">The template name.</param>
/// <param name="Variables">The template variables.</param>
public sealed record SendEmailCommand(
    Guid OrderId,
    string To,
    string Template,
    IReadOnlyDictionary<string, string> Variables);

/// <summary>
/// Represents a command to send an SMS notification.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="To">The recipient phone number.</param>
/// <param name="Template">The template name.</param>
/// <param name="Variables">The template variables.</param>
public sealed record SendSmsCommand(
    Guid OrderId,
    string To,
    string Template,
    IReadOnlyDictionary<string, string> Variables);
