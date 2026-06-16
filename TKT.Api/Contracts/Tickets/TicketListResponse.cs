namespace TKT.Api.Contracts.Tickets;

public sealed record TicketListResponse(
    IReadOnlyList<TicketListItemResponse> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyDictionary<string, int> StatusCounts);
