namespace TKT.Core.Domain.Entities;

public class TicketComment
{
    public Guid CommentId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid TicketId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? ReplyToId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
