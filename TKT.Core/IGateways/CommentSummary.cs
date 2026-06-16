namespace TKT.Core.IGateways;

public sealed record CommentSummary(
    Guid CommentId,
    Guid TicketId,
    Guid AccountId,
    Guid? ReplyToId,
    string CommentText,
    bool IsInternal,
    DateTimeOffset? EditedAt,
    DateTimeOffset CreatedAt);
