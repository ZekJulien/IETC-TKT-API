namespace TKT.Api.Contracts.Account;

public sealed record MeResponse(
    string Email,
    bool EmailConfirmed,
    bool OnboardingRequired,
    IReadOnlyList<MembershipResponse> Memberships);

public sealed record MembershipResponse(Guid CompanyId, string Role);
