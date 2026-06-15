using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class CompanyMembersRepository(IDbSession db) : ICompanyMembersRepository
{
    private readonly IDbSession _db = db;

    public Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId)
    {
        const string sql = """
                           SELECT role
                           FROM company_members
                           WHERE company_id = @CompanyId AND account_id = @AccountId AND is_active = TRUE;
                           """;
        return _db.QuerySingleOrDefaultAsync<string>(sql, new { CompanyId = companyId, AccountId = accountId });
    }

    public Task<bool> MemberExistsAsync(Guid companyId, Guid accountId)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM company_members WHERE company_id = @CompanyId AND account_id = @AccountId);";
        return _db.ExecuteScalarAsync<bool>(sql, new { CompanyId = companyId, AccountId = accountId });
    }

    public Task AddMemberAsync(CompanyMemberRow member)
    {
        const string sql = """
                           INSERT INTO company_members (membership_id, company_id, account_id, role,
                                                        invited_by, department, job_title, joined_at)
                           VALUES (@MembershipId, @CompanyId, @AccountId, @Role,
                                   @InvitedBy, @Department, @JobTitle, @JoinedAt);
                           """;
        return _db.ExecuteAsync(sql, member);
    }

    public Task<int> CountActiveMembersAsync(Guid companyId)
    {
        const string sql = "SELECT COUNT(*)::int FROM company_members WHERE company_id = @CompanyId AND is_active = TRUE;";
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }
}
