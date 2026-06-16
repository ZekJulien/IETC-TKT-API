namespace TKT.Core.UseCases.Comments.UpdateComment;

public sealed record UpdateCommentInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid CommentId,
    string Content);
