using TKT.Api.Contracts.Onboarding;
using TKT.Core.UseCases.Onboarding.CreateCompany;
using TKT.Core.UseCases.Onboarding.JoinInvitation;

namespace TKT.Api.Mappers;

public static class OnboardingMapper
{
    public static CreateCompanyInput ToInput(this CreateCompanyRequest request, Guid accountId, string email)
        => new(accountId, email, request.CompanyName, request.CompanySlug);

    public static JoinInvitationInput ToInput(this JoinInvitationRequest request, Guid accountId, string email)
        => new(accountId, email, request.InvitationCode);

    public static CreateCompanyResponse ToResponse(this CreateCompanyResult result)
        => new(result.CompanyId, result.CompanyName, result.CompanySlug, result.Role, result.AccessToken, result.RefreshToken);

    public static JoinInvitationResponse ToResponse(this JoinInvitationResult result)
        => new(result.CompanyId, result.Role, result.AccessToken, result.RefreshToken);
}
