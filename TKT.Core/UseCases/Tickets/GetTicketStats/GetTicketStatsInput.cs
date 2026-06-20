namespace TKT.Core.UseCases.Tickets.GetTicketStats;

public sealed record GetTicketStatsInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId);
