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
                           INSERT INTO refresh_tokens (token_id, account_id, token_hash, expires_at, absolute_expires_at)
                           VALUES (@TokenId, @AccountId, @TokenHash, @ExpiresAt, @AbsoluteExpiresAt);
                           """;
        return _db.ExecuteAsync(sql, row);
    }
}
