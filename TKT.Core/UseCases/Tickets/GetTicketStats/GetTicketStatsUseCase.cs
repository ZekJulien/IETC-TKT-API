using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.GetTicketStats;

public sealed class GetTicketStatsUseCase(
    ICompanyMembersGateway members,
    ITicketsGateway tickets) : IGetTicketStatsUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<TicketStats> ExecuteAsync(GetTicketStatsInput input)
    {
        if (input.CallerCompanyId is not { } companyId)
            throw new ForbiddenException(TicketErrors.Forbidden);

        var role = await _members.GetActiveRoleAsync(companyId, input.CallerAccountId);
        if (!TicketAuthorizationPolicy.CanList(role))
            throw new ForbiddenException(TicketErrors.Forbidden);

        var restrictToOwn = TicketAuthorizationPolicy.RestrictsToOwnTickets(role);

        return await _tickets.GetStatsAsync(companyId, input.CallerAccountId, restrictToOwn);
    }
}
