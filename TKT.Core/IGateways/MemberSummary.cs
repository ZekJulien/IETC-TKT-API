namespace TKT.Core.IGateways;

public sealed record MemberSummary(
    Guid AccountId,
    string Email,
    string? DisplayName,
    string Role,
    bool IsActive,
    DateTimeOffset? JoinedAt);
