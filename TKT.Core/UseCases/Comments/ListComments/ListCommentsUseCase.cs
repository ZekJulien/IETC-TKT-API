using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Comments.ListComments;

public sealed class ListCommentsUseCase(
    ICompanyMembersGateway members,
    ITicketsGateway tickets,
    ICommentsGateway comments) : IListCommentsUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ITicketsGateway _tickets = tickets;
    private readonly ICommentsGateway _comments = comments;

    public async Task<IReadOnlyList<CommentSummary>> ExecuteAsync(ListCommentsInput input)
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

        var includeInternal = TicketAuthorizationPolicy.CanViewInternal(role);
        return await _comments.ListByTicketAsync(companyId, input.TicketId, includeInternal);
    }
}
