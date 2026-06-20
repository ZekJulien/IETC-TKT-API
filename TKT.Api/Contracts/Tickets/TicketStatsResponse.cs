namespace TKT.Api.Contracts.Tickets;

public sealed record TicketStatsResponse(
    int Total,
    int Open,
    int InProgress,
    int Pending,
    int Resolved,
    int Closed,
    int Unassigned);
