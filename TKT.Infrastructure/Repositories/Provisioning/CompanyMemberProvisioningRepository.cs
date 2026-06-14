using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories.Provisioning;

public class CompanyMemberProvisioningRepository(ISystemDbSession db) : ICompanyMemberProvisioningRepository
{
    private readonly ISystemDbSession _db = db;

    public Task<bool> MemberExistsAsync(Guid companyId, Guid accountId)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM company_members WHERE company_id = @CompanyId AND account_id = @AccountId)";
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
}
