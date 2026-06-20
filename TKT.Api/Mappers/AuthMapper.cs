using TKT.Api.Contracts.Auth;
using TKT.Core.UseCases.Auth.ConfirmEmail;
using TKT.Core.UseCases.Auth.Login;
using TKT.Core.UseCases.Auth.Refresh;
using TKT.Core.UseCases.Auth.Register;
using TKT.Core.UseCases.Auth.SwitchTenant;

namespace TKT.Api.Mappers;

public static class AuthMapper
{
    public static RegisterInput ToInput(this RegisterRequest request)
        => new(request.Email, request.Password, request.ConfirmPassword, request.FirstName, request.LastName);

    public static LoginInput ToInput(this LoginRequest request)
        => new(request.Email, request.Password);

    public static RefreshInput ToInput(this RefreshRequest request)
        => new(request.RefreshToken);

    public static SwitchTenantInput ToInput(this SwitchTenantRequest request, Guid accountId, string email)
        => new(accountId, email, request.CompanyId);

    public static ConfirmEmailResponse ToResponse(this ConfirmEmailResult result)
        => new(result.AccessToken);

    public static TokenPairResponse ToResponse(this LoginResult result)
        => new(result.AccessToken, result.RefreshToken);

    public static TokenPairResponse ToResponse(this RefreshResult result)
        => new(result.AccessToken, result.RefreshToken);

    public static TokenPairResponse ToResponse(this SwitchTenantResult result)
        => new(result.AccessToken, result.RefreshToken);
}
