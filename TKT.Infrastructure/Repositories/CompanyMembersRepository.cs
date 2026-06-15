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

    private const string TeamUnionSql = """
                                        SELECT cm.account_id AS account_id, NULL::uuid AS invitation_id,
                                               a.email AS email, up.display_name AS display_name, cm.role AS role,
                                               CASE WHEN cm.is_active THEN 'active' ELSE 'inactive' END AS status,
                                               cm.joined_at AS joined_at
                                        FROM company_members cm
                                        JOIN accounts a ON a.account_id = cm.account_id
                                        LEFT JOIN user_profiles up ON up.account_id = cm.account_id
                                        WHERE cm.company_id = @CompanyId
                                        UNION ALL
                                        SELECT NULL::uuid AS account_id, pi.invitation_id AS invitation_id,
                                               pi.email AS email, NULL AS display_name, pi.role AS role,
                                               'pending' AS status, pi.invited_at AS joined_at
                                        FROM pending_invitations pi
                                        WHERE pi.company_id = @CompanyId
                                          AND pi.accepted_at IS NULL AND pi.revoked_at IS NULL AND pi.expires_at > NOW()
                                        """;

    private const string TeamFilterSql = """
                                          WHERE (@Role::text IS NULL OR t.role = @Role::text)
                                            AND (@IsActive::boolean IS NULL
                                                 OR t.status = CASE WHEN @IsActive::boolean THEN 'active' ELSE 'inactive' END)
                                          """;

    public Task<IReadOnlyList<MemberSummaryRow>> ListAsync(Guid companyId, int page, int pageSize, string? role, bool? isActive)
    {
        var sql = $"""
                  SELECT t.account_id, t.invitation_id, t.email, t.display_name, t.role, t.status, t.joined_at
                  FROM ({TeamUnionSql}) t
                  {TeamFilterSql}
                  ORDER BY t.joined_at NULLS LAST, t.email
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
        var sql = $"""
                  SELECT COUNT(*)::int
                  FROM ({TeamUnionSql}) t
                  {TeamFilterSql};
                  """;
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId, Role = role, IsActive = isActive });
    }
}
