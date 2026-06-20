using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Tickets.GetTicketStats;

public sealed class GetTicketStatsUseCase(
    ICompanyMemberAuthorizer authorizer,
    ITicketsGateway tickets) : IGetTicketStatsUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<TicketStats> ExecuteAsync(GetTicketStatsInput input)
    {
        var caller = await _authorizer.ResolveAsync(input.CallerCompanyId, input.CallerAccountId);
        if (caller is null || !TicketAuthorizationPolicy.CanList(caller.Role))
            throw new ForbiddenException(TicketErrors.Forbidden);
        var companyId = caller.CompanyId;

        var restrictToOwn = TicketAuthorizationPolicy.RestrictsToOwnTickets(caller.Role);

        return await _tickets.GetStatsAsync(companyId, input.CallerAccountId, restrictToOwn);
    }
}
