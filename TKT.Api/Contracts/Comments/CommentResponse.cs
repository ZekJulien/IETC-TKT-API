namespace TKT.Api.Contracts.Comments;

public sealed record CommentResponse(
    Guid CommentId,
    Guid TicketId,
    Guid AccountId,
    Guid? ReplyToId,
    string Content,
    bool IsInternal,
    DateTimeOffset? EditedAt,
    DateTimeOffset CreatedAt);
