using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Comments.CreateComment;

public sealed class CreateCommentUseCase(
    ICompanyMembersGateway members,
    ITicketsGateway tickets,
    ICommentsGateway comments) : ICreateCommentUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ITicketsGateway _tickets = tickets;
    private readonly ICommentsGateway _comments = comments;

    public async Task<CommentSummary> ExecuteAsync(CreateCommentInput input)
    {
        if (input.CallerCompanyId is not { } companyId)
            throw new ForbiddenException(CommentErrors.Forbidden);

        var role = await _members.GetActiveRoleAsync(companyId, input.CallerAccountId);
        if (!TicketAuthorizationPolicy.CanList(role))
            throw new ForbiddenException(CommentErrors.Forbidden);

        var ticket = await _tickets.GetByIdAsync(companyId, input.TicketId);
        if (ticket is null)
            throw new NotFoundException(CommentErrors.NotFound);
        if (TicketAuthorizationPolicy.RestrictsToOwnTickets(role) && ticket.CreatedBy != input.CallerAccountId)
            throw new NotFoundException(CommentErrors.NotFound);

        var canViewInternal = TicketAuthorizationPolicy.CanViewInternal(role);
        if (input.IsInternal && !canViewInternal)
            throw new ForbiddenException(CommentErrors.Forbidden);

        var text = CommentText.Create(input.Content);

        if (input.ReplyToId is { } replyToId
            && !await _comments.ReplyTargetExistsAsync(companyId, input.TicketId, replyToId, canViewInternal))
            throw new ValidationException(CommentErrors.ReplyInvalid);

        var comment = new TicketComment
        {
            CommentId = Guid.CreateVersion7(),
            CompanyId = companyId,
            TicketId = input.TicketId,
            AccountId = input.CallerAccountId,
            ReplyToId = input.ReplyToId,
            CommentText = text.Value,
            IsInternal = input.IsInternal,
        };

        return await _comments.CreateAsync(comment);
    }
}
