namespace TKT.Infrastructure.Models;

public sealed class TicketStatsRow
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int InProgress { get; set; }
    public int Pending { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    public int Unassigned { get; set; }
}
