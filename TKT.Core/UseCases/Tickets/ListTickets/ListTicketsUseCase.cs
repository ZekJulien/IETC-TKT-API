using TKT.Core.Common;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.ListTickets;

public sealed class ListTicketsUseCase(
    ICompanyMembersGateway members,
    ITicketsGateway tickets) : IListTicketsUseCase
{
    private static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
    {
        "created_at", "priority", "status",
    };

    private readonly ICompanyMembersGateway _members = members;
    private readonly ITicketsGateway _tickets = tickets;

    public async Task<ListTicketsResult> ExecuteAsync(ListTicketsInput input)
    {
        if (input.CallerCompanyId is not { } companyId)
            throw new ForbiddenException(TicketErrors.Forbidden);

        var role = await _members.GetActiveRoleAsync(companyId, input.CallerAccountId);
        if (!TicketAuthorizationPolicy.CanList(role))
            throw new ForbiddenException(TicketErrors.Forbidden);

        var sort = NormalizeSort(input.Sort);
        var pagination = Pagination.Create(input.Page, input.PageSize);

        var query = new TicketListQuery(
            companyId,
            input.CallerAccountId,
            TicketAuthorizationPolicy.RestrictsToOwnTickets(role),
            NormalizeFilter(input.Status),
            NormalizeFilter(input.Priority),
            input.AssignedTo,
            input.UnassignedOnly,
            input.CategoryId,
            sort,
            pagination.Page,
            pagination.PageSize);

        var page = await _tickets.ListAsync(query);
        var statusCounts = await _tickets.CountByStatusAsync(query);

        return new ListTicketsResult(page, statusCounts);
    }

    private static string NormalizeSort(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "created_at";
        var sort = raw.Trim().ToLowerInvariant();
        if (!AllowedSorts.Contains(sort)) throw new ValidationException(TicketErrors.SortInvalid);
        return sort;
    }

    private static string? NormalizeFilter(string? raw)
        => string.IsNullOrWhiteSpace(raw) ? null : raw.Trim().ToLowerInvariant();
}
