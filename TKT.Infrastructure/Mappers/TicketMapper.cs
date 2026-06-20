using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class TicketMapper
{
    public static TicketRow ToRow(this Ticket ticket)
        => new()
        {
            TicketId = ticket.TicketId,
            CompanyId = ticket.CompanyId,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedBy = ticket.CreatedBy,
            AssignedTo = ticket.AssignedTo,
            CategoryId = ticket.CategoryId,
            Source = ticket.Source,
            DueDate = ticket.DueDate,
        };

    public static TicketCreated ToCreated(this TicketCreatedRow row)
        => new(row.TicketNumber, row.CreatedAt);

    public static TicketSummary ToSummary(this TicketSummaryRow row)
        => new(row.TicketId, row.TicketNumber, row.Title, row.Status, row.Priority,
               row.CreatedBy, row.AssignedTo, row.CategoryId, row.CreatedAt);

    public static StatusCount ToStatusCount(this StatusCountRow row)
        => new(row.Status, row.Count);

    public static TicketStats ToStats(this TicketStatsRow row)
        => new(row.Total, row.Open, row.InProgress, row.Pending, row.Resolved, row.Closed, row.Unassigned);

    public static TicketDetail ToDetail(this TicketDetailRow row)
        => new(row.TicketId, row.TicketNumber, row.Title, row.Description, row.Status, row.Priority,
               row.CreatedBy, row.AssignedTo, row.TeamId, row.CategoryId, row.Source, row.DueDate,
               row.IsLocked, row.CreatedAt, row.UpdatedAt, row.ResolvedAt, row.ClosedAt);
}
