namespace TKT.Core.UseCases.Tickets.ListTickets;

public sealed record ListTicketsInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    string? Status,
    string? Priority,
    Guid? AssignedTo,
    bool UnassignedOnly,
    Guid? CategoryId,
    string? Sort,
    int Page,
    int PageSize);
