namespace TKT.Core.IGateways;

public sealed record TicketListQuery(
    Guid CompanyId,
    Guid CurrentUserId,
    bool RestrictToOwn,
    string? Status,
    string? Priority,
    Guid? AssignedTo,
    Guid? CategoryId,
    string Sort,
    int Page,
    int PageSize);
