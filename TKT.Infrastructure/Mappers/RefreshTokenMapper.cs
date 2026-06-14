using TKT.Core.Domain.Entities;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class RefreshTokenMapper
{
    public static RefreshTokenRow ToRow(this RefreshToken token)
    {
        return new RefreshTokenRow
        {
            TokenId = token.TokenId,
            AccountId = token.AccountId,
            TokenHash = token.TokenHash,
            ExpiresAt = token.ExpiresAt,
            AbsoluteExpiresAt = token.AbsoluteExpiresAt,
        };
    }
}
