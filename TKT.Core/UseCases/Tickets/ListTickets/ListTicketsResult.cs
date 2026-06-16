using TKT.Core.Common;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.ListTickets;

public sealed record ListTicketsResult(
    PagedResult<TicketSummary> Tickets,
    IReadOnlyList<StatusCount> StatusCounts);
