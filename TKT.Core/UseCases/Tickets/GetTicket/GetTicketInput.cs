namespace TKT.Core.UseCases.Tickets.GetTicket;

public sealed record GetTicketInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid TicketId);
