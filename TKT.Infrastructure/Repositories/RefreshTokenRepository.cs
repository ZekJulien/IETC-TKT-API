using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class RefreshTokenRepository(IDbSession db) : IRefreshTokenRepository
{
    private readonly IDbSession _db = db;

    public Task AddAsync(RefreshTokenRow row)
    {
        const string sql = """
                           INSERT INTO refresh_tokens
                               (token_id, account_id, family_id, token_hash, expires_at, absolute_expires_at)
                           VALUES
                               (@TokenId, @AccountId, @FamilyId, @TokenHash, @ExpiresAt, @AbsoluteExpiresAt);
                           """;
        return _db.ExecuteAsync(sql, row);
    }

    public Task<RefreshTokenRow?> GetByHashAsync(string tokenHash)
    {
        const string sql = """
                           SELECT token_id, account_id, family_id, token_hash, replaced_by_id,
                                  expires_at, absolute_expires_at, is_revoked
                           FROM refresh_tokens
                           WHERE token_hash = @TokenHash;
                           """;
        return _db.QuerySingleOrDefaultAsync<RefreshTokenRow>(sql, new { TokenHash = tokenHash });
    }

    public Task MarkRotatedAsync(Guid tokenId, Guid replacedById)
    {
        const string sql = """
                           UPDATE refresh_tokens
                           SET is_revoked = TRUE, revoked_at = NOW(), revoked_reason = 'rotated',
                               replaced_by_id = @ReplacedById
                           WHERE token_id = @TokenId;
                           """;
        return _db.ExecuteAsync(sql, new { TokenId = tokenId, ReplacedById = replacedById });
    }
}
