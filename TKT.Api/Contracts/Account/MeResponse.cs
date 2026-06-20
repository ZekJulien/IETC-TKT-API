namespace TKT.Api.Contracts.Account;

public sealed record MeResponse(
    string Email,
    bool EmailConfirmed,
    bool OnboardingRequired,
    string? FirstName,
    string? LastName,
    IReadOnlyList<MembershipResponse> Memberships);

public sealed record MembershipResponse(Guid CompanyId, string Role);
