using TKT.Core.Domain.Entities;

namespace TKT.Core.UseCases.Identity.GetMe;

public sealed record GetMeResult(
    string Email,
    bool EmailConfirmed,
    bool OnboardingRequired,
    IReadOnlyList<CompanyMember> Memberships);
