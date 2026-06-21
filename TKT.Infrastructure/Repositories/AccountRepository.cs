using TKT.Infrastructure.Persistence.Abstractions;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class AccountRepository(IDbSession db) : IAccountRepository
{
    private readonly IDbSession _db = db;

    public Task AddAccount(AccountRow accountRow)
    {
        const string sql = """
                           INSERT INTO accounts (account_id, email, normalized_email, password_hash, security_stamp)
                           VALUES (@AccountId, @Email, @NormalizedEmail, @PasswordHash, @SecurityStamp);
                           """;
        return _db.ExecuteAsync(sql, accountRow);
    }

    public Task<bool> ExistByEmailAsync(string normalizedEmail)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM accounts WHERE normalized_email = @NormalizedEmail)";
        return _db.ExecuteScalarAsync<bool>(sql, new { NormalizedEmail = normalizedEmail });
    }

    public Task<AccountRow?> GetByIdAsync(Guid accountId)
    {
        const string sql = """
                           SELECT account_id, email, normalized_email, password_hash,
                                  security_stamp, email_confirmed, is_active
                           FROM accounts
                           WHERE account_id = @AccountId;
                           """;
        return _db.QuerySingleOrDefaultAsync<AccountRow>(sql, new { AccountId = accountId });
    }

    public Task<AccountRow?> GetByNormalizedEmailAsync(string normalizedEmail)
    {
        const string sql = """
                           SELECT account_id, email, normalized_email, password_hash, security_stamp,
                                  email_confirmed, is_active, access_failed_count, lockout_end
                           FROM accounts
                           WHERE normalized_email = @NormalizedEmail;
                           """;
        return _db.QuerySingleOrDefaultAsync<AccountRow>(sql, new { NormalizedEmail = normalizedEmail });
    }

    public Task SetEmailConfirmedAsync(Guid accountId)
    {
        const string sql = "UPDATE accounts SET email_confirmed = TRUE WHERE account_id = @AccountId;";
        return _db.ExecuteAsync(sql, new { AccountId = accountId });
    }

    public Task ResetLockoutAsync(Guid accountId)
    {
        const string sql = """
                           UPDATE accounts
                           SET access_failed_count = 0, lockout_end = NULL
                           WHERE account_id = @AccountId;
                           """;
        return _db.ExecuteAsync(sql, new { AccountId = accountId });
    }
}
