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

    public Task<IReadOnlyList<MemberCompanyRow>> GetCompaniesForAccountAsync(Guid accountId)
    {
        const string sql = """
                           SELECT c.company_id, c.company_name, c.company_slug, c.logo_url,
                                  cm.role, cm.is_active
                           FROM company_members cm
                           JOIN companies c ON c.company_id = cm.company_id
                           WHERE cm.account_id = @AccountId AND c.deleted_at IS NULL
                           ORDER BY c.company_name;
                           """;
        return _db.QueryAsync<MemberCompanyRow>(sql, new { AccountId = accountId });
    }
}
