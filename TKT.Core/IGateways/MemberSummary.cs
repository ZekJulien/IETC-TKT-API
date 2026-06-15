namespace TKT.Core.IGateways;

public sealed record MemberSummary(
    Guid? AccountId,
    Guid? InvitationId,
    string Email,
    string? DisplayName,
    string Role,
    string Status,
    DateTimeOffset? JoinedAt);
