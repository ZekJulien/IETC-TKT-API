namespace TKT.Infrastructure.Models;

public sealed class TicketDetailRow
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset? DueDate { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
