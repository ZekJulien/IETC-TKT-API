namespace TKT.Core.UseCases.Tickets.CreateTicket;

public sealed record CreateTicketResult(
    Guid TicketId,
    string TicketNumber,
    string Status,
    string Priority,
    DateTimeOffset CreatedAt);
