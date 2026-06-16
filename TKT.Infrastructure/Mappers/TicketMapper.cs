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
        };

    public static TicketCreated ToCreated(this TicketCreatedRow row)
        => new(row.TicketNumber, row.CreatedAt);
}
