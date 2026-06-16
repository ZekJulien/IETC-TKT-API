namespace TKT.Api.Contracts.Tickets;

public sealed record TicketListItemResponse(
    Guid TicketId,
    string TicketNumber,
    string Title,
    string Status,
    string Priority,
    Guid CreatedBy,
    Guid? AssignedTo,
    Guid? CategoryId,
    DateTimeOffset CreatedAt);
