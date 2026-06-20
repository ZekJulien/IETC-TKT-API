using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;

namespace TKT.Core.Services;

public sealed class RefreshTokenIssuer(
    IRefreshTokenService refreshTokenService,
    IRefreshTokenGateway refreshTokenGateway) : IRefreshTokenIssuer
{
    public async Task<string> IssueAsync(Guid accountId, Guid? companyId = null)
    {
        var refresh = refreshTokenService.Generate(accountId, companyId);
        await refreshTokenGateway.AddAsync(new RefreshToken
        {
            TokenId = Guid.CreateVersion7(),
            AccountId = accountId,
            FamilyId = Guid.CreateVersion7(),
            TokenHash = refresh.TokenHash,
            ExpiresAt = refresh.ExpiresAt,
            AbsoluteExpiresAt = refresh.AbsoluteExpiresAt,
        });
        return refresh.Token;
    }
}
