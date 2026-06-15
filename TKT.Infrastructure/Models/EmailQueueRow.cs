namespace TKT.Infrastructure.Models;

public sealed class EmailQueueRow
{
    public Guid QueueId { get; set; }
    public Guid CompanyId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string EmailType { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
}
