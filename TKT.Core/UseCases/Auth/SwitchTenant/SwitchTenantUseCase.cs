using TKT.Core.Abstractions;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Auth.SwitchTenant;

public sealed class SwitchTenantUseCase(
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenIssuer refreshTokenIssuer) : ISwitchTenantUseCase
{
    private readonly IAccessTokenIssuer _accessTokenIssuer = accessTokenIssuer;
    private readonly IRefreshTokenIssuer _refreshTokenIssuer = refreshTokenIssuer;

    public async Task<SwitchTenantResult> ExecuteAsync(SwitchTenantInput input)
    {
        var accessToken = await _accessTokenIssuer.IssueForCompanyAsync(input.AccountId, input.Email, input.CompanyId);
        var refreshToken = await _refreshTokenIssuer.IssueAsync(input.AccountId, input.CompanyId);

        return new SwitchTenantResult(accessToken, refreshToken);
    }
}
