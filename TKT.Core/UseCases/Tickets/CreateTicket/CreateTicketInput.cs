namespace TKT.Core.UseCases.Tickets.CreateTicket;

public sealed record CreateTicketInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    string Title,
    string? Description,
    string? Priority,
    Guid? CategoryId,
    Guid? AssignedTo,
    string? Source,
    DateTimeOffset? DueDate);
