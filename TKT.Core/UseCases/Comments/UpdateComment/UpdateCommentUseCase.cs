using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Comments.UpdateComment;

public sealed class UpdateCommentUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICommentsGateway comments) : IUpdateCommentUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICommentsGateway _comments = comments;

    public async Task<CommentSummary> ExecuteAsync(UpdateCommentInput input)
    {
        var caller = await _authorizer.ResolveAsync(input.CallerCompanyId, input.CallerAccountId);
        if (caller is null || caller.Role is null)
            throw new ForbiddenException(CommentErrors.Forbidden);
        var companyId = caller.CompanyId;

        var comment = await _comments.GetAsync(companyId, input.CommentId);
        if (comment is null)
            throw new NotFoundException(CommentErrors.NotFound);

        if (comment.AccountId != input.CallerAccountId)
            throw new ForbiddenException(CommentErrors.Forbidden);

        var text = CommentText.Create(input.Content);

        var updated = await _comments.UpdateTextAsync(companyId, input.CommentId, text.Value);
        if (updated is null)
            throw new NotFoundException(CommentErrors.NotFound);

        return updated;
    }
}
