using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Auth.SwitchTenant;

public sealed class SwitchTenantUseCase(
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenService refreshTokenService,
    IRefreshTokenGateway refreshTokenGateway) : ISwitchTenantUseCase
{
    private readonly IAccessTokenIssuer _accessTokenIssuer = accessTokenIssuer;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IRefreshTokenGateway _refreshTokenGateway = refreshTokenGateway;

    public async Task<SwitchTenantResult> ExecuteAsync(SwitchTenantInput input)
    {
        var accessToken = await _accessTokenIssuer.IssueForCompanyAsync(input.AccountId, input.Email, input.CompanyId);

        var refresh = _refreshTokenService.Generate(input.AccountId, input.CompanyId);
        await _refreshTokenGateway.AddAsync(new RefreshToken
        {
            TokenId = Guid.CreateVersion7(),
            AccountId = input.AccountId,
            FamilyId = Guid.CreateVersion7(),
            TokenHash = refresh.TokenHash,
            ExpiresAt = refresh.ExpiresAt,
            AbsoluteExpiresAt = refresh.AbsoluteExpiresAt,
        });

        return new SwitchTenantResult(accessToken, refresh.Token);
    }
}
