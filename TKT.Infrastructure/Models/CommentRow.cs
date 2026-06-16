namespace TKT.Infrastructure.Models;

public sealed class CommentRow
{
    public Guid CommentId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid TicketId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? ReplyToId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
