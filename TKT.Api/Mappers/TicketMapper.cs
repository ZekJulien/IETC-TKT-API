using TKT.Api.Contracts.Tickets;
using TKT.Core.IGateways;
using TKT.Core.UseCases.Tickets.CreateTicket;
using TKT.Core.UseCases.Tickets.ListTickets;
using TKT.Core.UseCases.Tickets.UpdateTicket;
using TKT.Core.UseCases.Tickets.GetTicketStats;

namespace TKT.Api.Mappers;

public static class TicketMapper
{
    public static CreateTicketInput ToInput(this CreateTicketRequest request, Guid? callerCompanyId, Guid callerAccountId)
        => new(callerCompanyId, callerAccountId, request.Title, request.Description, request.Priority, request.CategoryId, request.AssignedTo, request.Source, request.DueDate);

    public static CreateTicketResponse ToResponse(this CreateTicketResult result)
        => new(result.TicketId, result.TicketNumber, result.Status, result.Priority, result.CreatedAt);

    public static TicketListItemResponse ToResponse(this TicketSummary summary)
        => new(summary.TicketId, summary.TicketNumber, summary.Title, summary.Status, summary.Priority,
               summary.CreatedBy, summary.AssignedTo, summary.CategoryId, summary.CreatedAt);

    public static TicketListResponse ToResponse(this ListTicketsResult result)
    {
        var page = result.Tickets;
        var totalPages = page.PageSize > 0 ? (int)Math.Ceiling((double)page.Total / page.PageSize) : 0;
        var statusCounts = result.StatusCounts.ToDictionary(c => c.Status, c => c.Count);
        return new TicketListResponse(
            page.Items.Select(i => i.ToResponse()).ToList(),
            page.Total,
            page.Page,
            page.PageSize,
            totalPages,
            statusCounts);
    }

    public static UpdateTicketInput ToInput(this UpdateTicketRequest request, Guid? callerCompanyId, Guid callerAccountId, Guid ticketId)
        => new(callerCompanyId, callerAccountId, ticketId, request.Status, request.Priority, request.AssignedTo, request.CategoryId, request.DueDate);

    public static TicketStatsResponse ToResponse(this TicketStats stats)
        => new(stats.Total, stats.Open, stats.InProgress, stats.Pending, stats.Resolved, stats.Closed, stats.Unassigned);

    public static TicketDetailResponse ToResponse(this TicketDetail detail)
        => new(detail.TicketId, detail.TicketNumber, detail.Title, detail.Description, detail.Status, detail.Priority,
               detail.CreatedBy, detail.AssignedTo, detail.TeamId, detail.CategoryId, detail.Source, detail.DueDate,
               detail.IsLocked, detail.CreatedAt, detail.UpdatedAt, detail.ResolvedAt, detail.ClosedAt);
}
