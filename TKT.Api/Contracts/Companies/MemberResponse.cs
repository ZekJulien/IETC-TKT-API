namespace TKT.Api.Contracts.Companies;

public sealed record MemberResponse(
    Guid AccountId,
    string Email,
    string? DisplayName,
    string Role,
    bool IsActive,
    DateTimeOffset? JoinedAt);
