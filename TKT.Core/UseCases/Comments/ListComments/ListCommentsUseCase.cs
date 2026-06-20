using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Comments.ListComments;

public sealed class ListCommentsUseCase(
    ICompanyMemberAuthorizer authorizer,
    ITicketsGateway tickets,
    ICommentsGateway comments) : IListCommentsUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ITicketsGateway _tickets = tickets;
    private readonly ICommentsGateway _comments = comments;

    public async Task<IReadOnlyList<CommentSummary>> ExecuteAsync(ListCommentsInput input)
    {
        var caller = await _authorizer.ResolveAsync(input.CallerCompanyId, input.CallerAccountId);
        if (caller is null || !TicketAuthorizationPolicy.CanList(caller.Role))
            throw new ForbiddenException(CommentErrors.Forbidden);
        var companyId = caller.CompanyId;
        var role = caller.Role;

        var ticket = await _tickets.GetByIdAsync(companyId, input.TicketId);
        if (ticket is null)
            throw new NotFoundException(CommentErrors.NotFound);
        if (TicketAuthorizationPolicy.RestrictsToOwnTickets(role) && ticket.CreatedBy != input.CallerAccountId)
            throw new NotFoundException(CommentErrors.NotFound);

        var includeInternal = TicketAuthorizationPolicy.CanViewInternal(role);
        return await _comments.ListByTicketAsync(companyId, input.TicketId, includeInternal);
    }
}
