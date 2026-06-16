namespace TKT.Core.IGateways;

public sealed record TicketUpdate(
    Guid CompanyId,
    Guid TicketId,
    string? Status,
    string? Priority,
    Guid? AssignedTo,
    Guid? CategoryId,
    DateTimeOffset? DueDate);
