using Dapper;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class AccountRepository(IDbSession dbSession) : IAccountRepository
{
    private readonly IDbSession _dbSession = dbSession;

    public async Task AddAccount(AccountRow accountRow)
    {
        const string sql = """
                  INSERT INTO accounts (account_id,
                                        email,
                                        normalized_email,
                                        password_hash,
                                        security_stamp)
                  VALUES (@AccountId,
                          @Email,
                          @NormalizedEmail,
                          @PasswordHash,
                          @SecurityStamp);
                  """;
        var conn = await _dbSession.GetConnectionAsync();
        await conn.ExecuteAsync(sql, accountRow);
    }

    public async Task<bool> ExistByEmailAsync(string normalizedEmail)
    {
        const string sql = """
                            SELECT EXISTS(SELECT 1 FROM accounts
                            WHERE normalized_email = @NormalizedEmail)
                           """;
        var conn = await _dbSession.GetConnectionAsync();
        return await conn.ExecuteScalarAsync<bool>(sql, new { NormalizedEmail = normalizedEmail });
    }
}