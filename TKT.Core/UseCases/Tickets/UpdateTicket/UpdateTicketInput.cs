namespace TKT.Core.UseCases.Tickets.UpdateTicket;

public sealed record UpdateTicketInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid TicketId,
    string? Status,
    string? Priority,
    Guid? AssignedTo,
    Guid? CategoryId,
    DateTimeOffset? DueDate);
