namespace TKT.Core.IGateways;

public sealed record QueuedEmail(
    Guid CompanyId,
    string RecipientEmail,
    string Subject,
    string BodyHtml,
    string EmailType,
    string EntityType,
    Guid EntityId);
