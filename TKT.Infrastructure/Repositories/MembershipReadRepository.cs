using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class MembershipReadRepository(IDbSession db) : IMembershipReadRepository
{
    private readonly IDbSession _db = db;

    public Task<IReadOnlyList<CompanyMemberRow>> GetActiveForAccountAsync(Guid accountId)
    {
        const string sql = """
                           SELECT company_id, role
                           FROM company_members
                           WHERE account_id = @AccountId AND is_active = TRUE;
                           """;
        return _db.QueryAsync<CompanyMemberRow>(sql, new { AccountId = accountId });
    }
}
