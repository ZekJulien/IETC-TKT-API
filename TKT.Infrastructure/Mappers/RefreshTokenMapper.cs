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
            FamilyId = token.FamilyId,
            TokenHash = token.TokenHash,
            ExpiresAt = token.ExpiresAt,
            AbsoluteExpiresAt = token.AbsoluteExpiresAt,
        };
    }

    public static RefreshToken ToDomain(this RefreshTokenRow row)
    {
        return new RefreshToken
        {
            TokenId = row.TokenId,
            AccountId = row.AccountId,
            FamilyId = row.FamilyId,
            TokenHash = row.TokenHash,
            ReplacedById = row.ReplacedById,
            ExpiresAt = row.ExpiresAt,
            AbsoluteExpiresAt = row.AbsoluteExpiresAt,
            IsRevoked = row.IsRevoked,
        };
    }
}
