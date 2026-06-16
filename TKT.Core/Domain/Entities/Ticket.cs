using TKT.Core.Domain;

namespace TKT.Core.Domain.Entities;

public class Ticket
{
    public Guid TicketId { get; set; }
    public Guid CompanyId { get; set; }
    public string? TicketNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = TicketStatuses.Open;
    public string Priority { get; set; } = "medium";
    public Guid CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? CategoryId { get; set; }
    public string Source { get; set; } = "web";
}
