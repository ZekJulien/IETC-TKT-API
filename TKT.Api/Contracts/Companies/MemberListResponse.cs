namespace TKT.Api.Contracts.Companies;

public sealed record MemberListResponse(
    IReadOnlyList<MemberResponse> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages,
    int ActiveMembers,
    int PendingInvitations,
    int MaxUsers);
