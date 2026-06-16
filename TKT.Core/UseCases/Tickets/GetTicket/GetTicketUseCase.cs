using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.GetTicket;

public sealed class GetTicketUseCase(
    ICompanyMembersGateway members,
    ITicketsGateway tickets) : IGetTicketUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<TicketDetail> ExecuteAsync(GetTicketInput input)
    {
        if (input.CallerCompanyId is not { } companyId)
            throw new ForbiddenException(TicketErrors.Forbidden);

        var role = await _members.GetActiveRoleAsync(companyId, input.CallerAccountId);
        if (!TicketAuthorizationPolicy.CanList(role))
            throw new ForbiddenException(TicketErrors.Forbidden);

        var detail = await _tickets.GetByIdAsync(companyId, input.TicketId);
        if (detail is null)
            throw new NotFoundException(TicketErrors.NotFound);

        if (TicketAuthorizationPolicy.RestrictsToOwnTickets(role) && detail.CreatedBy != input.CallerAccountId)
            throw new NotFoundException(TicketErrors.NotFound);

        return detail;
    }
}
