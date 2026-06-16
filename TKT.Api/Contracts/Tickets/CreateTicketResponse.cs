namespace TKT.Api.Contracts.Tickets;

public sealed record CreateTicketResponse(
    Guid TicketId,
    string TicketNumber,
    string Status,
    string Priority,
    DateTimeOffset CreatedAt);
