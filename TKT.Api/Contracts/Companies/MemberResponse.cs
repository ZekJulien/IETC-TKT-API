namespace TKT.Api.Contracts.Companies;

public sealed record MemberResponse(
    Guid? AccountId,
    Guid? InvitationId,
    string Email,
    string? DisplayName,
    string Role,
    string Status,
    DateTimeOffset? JoinedAt);
