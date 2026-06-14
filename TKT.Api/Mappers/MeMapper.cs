using TKT.Api.Contracts.Account;
using TKT.Core.UseCases.Identity.GetMe;

namespace TKT.Api.Mappers;

public static class MeMapper
{
    public static MeResponse ToResponse(this GetMeResult result)
        => new(
            result.Email,
            result.EmailConfirmed,
            result.OnboardingRequired,
            result.Memberships.Select(m => new MembershipResponse(m.CompanyId, m.Role)).ToList());
}
