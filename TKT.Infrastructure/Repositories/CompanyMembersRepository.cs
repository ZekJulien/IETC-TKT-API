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

    public Task<CompanyMemberRow?> GetMemberAsync(Guid companyId, Guid accountId)
    {
        const string sql = """
                           SELECT membership_id, company_id, account_id, role, is_active,
                                  invited_by, department, job_title, joined_at
                           FROM company_members
                           WHERE company_id = @CompanyId AND account_id = @AccountId;
                           """;
        return _db.QuerySingleOrDefaultAsync<CompanyMemberRow>(sql, new { CompanyId = companyId, AccountId = accountId });
    }

    public Task<int> CountActiveOwnersAsync(Guid companyId)
    {
        const string sql = "SELECT COUNT(*)::int FROM company_members WHERE company_id = @CompanyId AND role = 'owner' AND is_active = TRUE;";
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public Task UpdateRoleAsync(Guid companyId, Guid accountId, string role)
    {
        const string sql = "UPDATE company_members SET role = @Role WHERE company_id = @CompanyId AND account_id = @AccountId;";
        return _db.ExecuteAsync(sql, new { CompanyId = companyId, AccountId = accountId, Role = role });
    }

    public Task SetActiveAsync(Guid companyId, Guid accountId, bool isActive)
    {
        const string sql = """
                           UPDATE company_members
                           SET is_active = @IsActive,
                               deactivated_at = CASE WHEN @IsActive THEN NULL ELSE NOW() END
                           WHERE company_id = @CompanyId AND account_id = @AccountId;
                           """;
        return _db.ExecuteAsync(sql, new { CompanyId = companyId, AccountId = accountId, IsActive = isActive });
    }

    public Task<IReadOnlyList<MemberSummaryRow>> ListAsync(Guid companyId, int page, int pageSize, string? role, bool? isActive)
    {
        const string sql = """
                           SELECT cm.account_id, a.email, up.display_name, cm.role, cm.is_active, cm.joined_at
                           FROM company_members cm
                           JOIN accounts a ON a.account_id = cm.account_id
                           LEFT JOIN user_profiles up ON up.account_id = cm.account_id
                           WHERE cm.company_id = @CompanyId
                             AND (@Role::text IS NULL OR cm.role = @Role::text)
                             AND (@IsActive::boolean IS NULL OR cm.is_active = @IsActive::boolean)
                           ORDER BY cm.joined_at NULLS LAST, cm.account_id
                           LIMIT @PageSize OFFSET @Offset;
                           """;
        return _db.QueryAsync<MemberSummaryRow>(sql, new
        {
            CompanyId = companyId,
            Role = role,
            IsActive = isActive,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize,
        });
    }

    public Task<int> CountAsync(Guid companyId, string? role, bool? isActive)
    {
        const string sql = """
                           SELECT COUNT(*)::int
                           FROM company_members cm
                           WHERE cm.company_id = @CompanyId
                             AND (@Role::text IS NULL OR cm.role = @Role::text)
                             AND (@IsActive::boolean IS NULL OR cm.is_active = @IsActive::boolean);
                           """;
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId, Role = role, IsActive = isActive });
    }
}
