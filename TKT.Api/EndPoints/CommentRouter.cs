using System.Security.Claims;
using TKT.Api.Contracts.Comments;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Comments.CreateComment;
using TKT.Core.UseCases.Comments.ListComments;
using TKT.Core.UseCases.Comments.UpdateComment;

namespace TKT.Api.EndPoints;

public static class CommentRouter
{
    public static WebApplication AddCommentRouter(this WebApplication app)
    {
        app.MapPost("api/tickets/{ticketId:guid}/comments",
            async (Guid ticketId, CreateCommentRequest req, ClaimsPrincipal user, ICreateCommentUseCase useCase) =>
        {
            var comment = await useCase.ExecuteAsync(req.ToInput(user.GetCompanyId(), user.GetAccountId(), ticketId));
            return Results.Created($"/api/comments/{comment.CommentId}", comment.ToResponse());
        }).WithTags("comments");

        app.MapGet("api/tickets/{ticketId:guid}/comments",
            async (Guid ticketId, ClaimsPrincipal user, IListCommentsUseCase useCase) =>
        {
            var comments = await useCase.ExecuteAsync(new ListCommentsInput(user.GetCompanyId(), user.GetAccountId(), ticketId));
            return Results.Ok(comments.Select(c => c.ToResponse()).ToList());
        }).WithTags("comments");

        app.MapPatch("api/comments/{commentId:guid}",
            async (Guid commentId, UpdateCommentRequest req, ClaimsPrincipal user, IUpdateCommentUseCase useCase) =>
        {
            var comment = await useCase.ExecuteAsync(req.ToInput(user.GetCompanyId(), user.GetAccountId(), commentId));
            return Results.Ok(comment.ToResponse());
        }).WithTags("comments");

        return app;
    }
}
