using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class ProfileRepository(IDbSession db) : IProfileRepository
{
    private readonly IDbSession _db = db;

    public Task AddAsync(UserProfileRow profile)
    {
        const string sql = """
                           SELECT set_config('app.current_user_id', @AccountId::text, true);
                           INSERT INTO user_profiles (account_id, first_name, last_name)
                           VALUES (@AccountId, @FirstName, @LastName);
                           """;
        return _db.ExecuteAsync(sql, profile);
    }

    public Task<UserProfileRow?> GetByAccountIdAsync(Guid accountId)
    {
        const string sql = """
                           SELECT account_id, first_name, last_name
                           FROM user_profiles
                           WHERE account_id = @AccountId;
                           """;
        return _db.QuerySingleOrDefaultAsync<UserProfileRow>(sql, new { AccountId = accountId });
    }
}
