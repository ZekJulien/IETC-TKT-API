using System.Data.Common;
using Dapper;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class AccountLockoutRepository(IDbConnectionFactory factory) : IAccountLockoutRepository
{
    private readonly IDbConnectionFactory _factory = factory;

    public async Task RegisterFailedLoginAsync(Guid accountId, int failedCount, DateTimeOffset? lockoutEnd)
    {
        const string sql = """
                           UPDATE accounts
                           SET access_failed_count = @FailedCount, lockout_end = @LockoutEnd
                           WHERE account_id = @AccountId;
                           """;
        await using var connection = (DbConnection)await _factory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(new CommandDefinition(sql,
            new { AccountId = accountId, FailedCount = failedCount, LockoutEnd = lockoutEnd }));
    }
}
