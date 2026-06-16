using TKT.Api.Contracts.Comments;
using TKT.Core.IGateways;
using TKT.Core.UseCases.Comments.CreateComment;
using TKT.Core.UseCases.Comments.UpdateComment;

namespace TKT.Api.Mappers;

public static class CommentMapper
{
    public static CreateCommentInput ToInput(this CreateCommentRequest request, Guid? callerCompanyId, Guid callerAccountId, Guid ticketId)
        => new(callerCompanyId, callerAccountId, ticketId, request.Content, request.IsInternal, request.ReplyToId);

    public static UpdateCommentInput ToInput(this UpdateCommentRequest request, Guid? callerCompanyId, Guid callerAccountId, Guid commentId)
        => new(callerCompanyId, callerAccountId, commentId, request.Content);

    public static CommentResponse ToResponse(this CommentSummary comment)
        => new(comment.CommentId, comment.TicketId, comment.AccountId, comment.ReplyToId,
               comment.CommentText, comment.IsInternal, comment.EditedAt, comment.CreatedAt);
}
