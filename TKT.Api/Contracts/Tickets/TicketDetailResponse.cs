namespace TKT.Api.Contracts.Tickets;

public sealed record TicketDetailResponse(
    Guid TicketId,
    string TicketNumber,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid CreatedBy,
    Guid? AssignedTo,
    Guid? TeamId,
    Guid? CategoryId,
    string Source,
    DateTimeOffset? DueDate,
    bool IsLocked,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt);
