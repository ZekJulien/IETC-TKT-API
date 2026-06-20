using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Tickets.GetTicket;

public sealed class GetTicketUseCase(
    ICompanyMemberAuthorizer authorizer,
    ITicketsGateway tickets) : IGetTicketUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<TicketDetail> ExecuteAsync(GetTicketInput input)
    {
        var caller = await _authorizer.ResolveAsync(input.CallerCompanyId, input.CallerAccountId);
        if (caller is null || !TicketAuthorizationPolicy.CanList(caller.Role))
            throw new ForbiddenException(TicketErrors.Forbidden);
        var companyId = caller.CompanyId;
        var role = caller.Role;

        var detail = await _tickets.GetByIdAsync(companyId, input.TicketId);
        if (detail is null)
            throw new NotFoundException(TicketErrors.NotFound);

        if (TicketAuthorizationPolicy.RestrictsToOwnTickets(role) && detail.CreatedBy != input.CallerAccountId)
            throw new NotFoundException(TicketErrors.NotFound);

        return detail;
    }
}
