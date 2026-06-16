namespace TKT.Infrastructure.Models;

public sealed class TicketSummaryRow
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
