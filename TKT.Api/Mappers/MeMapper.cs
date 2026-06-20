using TKT.Api.Contracts.Account;
using TKT.Core.UseCases.Identity.GetMe;
using TKT.Core.UseCases.Identity.ListMyCompanies;

namespace TKT.Api.Mappers;

public static class MeMapper
{
    public static MeResponse ToResponse(this GetMeResult result)
        => new(
            result.Email,
            result.EmailConfirmed,
            result.OnboardingRequired,
            result.FirstName,
            result.LastName,
            result.Memberships.Select(m => new MembershipResponse(m.CompanyId, m.Role)).ToList());

    public static MyCompaniesResponse ToResponse(this ListMyCompaniesResult result)
        => new(result.Companies
            .Select(c => new MyCompanyResponse(c.CompanyId, c.CompanyName, c.CompanySlug, c.LogoUrl, c.Role, c.IsActive))
            .ToList());
}
