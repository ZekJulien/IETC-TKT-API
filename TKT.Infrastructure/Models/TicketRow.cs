namespace TKT.Infrastructure.Models;

public sealed class TicketRow
{
    public Guid TicketId { get; set; }
    public Guid CompanyId { get; set; }
    public string? TicketNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? CategoryId { get; set; }
    public string Source { get; set; } = string.Empty;
}
