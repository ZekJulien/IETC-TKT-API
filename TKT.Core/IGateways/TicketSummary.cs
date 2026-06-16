namespace TKT.Core.IGateways;

public sealed record TicketSummary(
    Guid TicketId,
    string TicketNumber,
    string Title,
    string Status,
    string Priority,
    Guid CreatedBy,
    Guid? AssignedTo,
    Guid? CategoryId,
    DateTimeOffset CreatedAt);
