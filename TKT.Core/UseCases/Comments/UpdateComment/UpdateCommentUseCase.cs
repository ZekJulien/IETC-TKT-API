using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Comments.UpdateComment;

public sealed class UpdateCommentUseCase(
    ICompanyMembersGateway members,
    ICommentsGateway comments) : IUpdateCommentUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICommentsGateway _comments = comments;

    public async Task<CommentSummary> ExecuteAsync(UpdateCommentInput input)
    {
        if (input.CallerCompanyId is not { } companyId)
            throw new ForbiddenException(CommentErrors.Forbidden);

        var role = await _members.GetActiveRoleAsync(companyId, input.CallerAccountId);
        if (role is null)
            throw new ForbiddenException(CommentErrors.Forbidden);

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
