namespace TKT.Core.UseCases.Comments.CreateComment;

public sealed record CreateCommentInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid TicketId,
    string Content,
    bool IsInternal,
    Guid? ReplyToId);
