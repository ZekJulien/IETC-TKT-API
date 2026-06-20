namespace TKT.Core.IGateways;

public sealed record TicketStats(
    int Total,
    int Open,
    int InProgress,
    int Pending,
    int Resolved,
    int Closed,
    int Unassigned);
