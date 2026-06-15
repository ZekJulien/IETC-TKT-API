using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class CompanyInvitationRepository(IDbSession db) : ICompanyInvitationRepository
{
    private readonly IDbSession _db = db;

    public Task<bool> HasActivePendingAsync(Guid companyId, string email)
    {
        const string sql = """
                           SELECT EXISTS(SELECT 1 FROM pending_invitations
                                         WHERE company_id = @CompanyId AND email = @Email
                                           AND accepted_at IS NULL AND revoked_at IS NULL AND expires_at > NOW());
                           """;
        return _db.ExecuteScalarAsync<bool>(sql, new { CompanyId = companyId, Email = email });
    }

    public Task CreateAsync(PendingInvitationRow invitation)
    {
        const string sql = """
                           INSERT INTO pending_invitations (invitation_id, company_id, email, role,
                                                            department, job_title, invitation_code, invited_by)
                           VALUES (@InvitationId, @CompanyId, @Email, @Role,
                                   @Department, @JobTitle, @InvitationCode, @InvitedBy);
                           """;
        return _db.ExecuteAsync(sql, invitation);
    }

    public Task<int> CountActivePendingAsync(Guid companyId)
    {
        const string sql = """
                           SELECT COUNT(*)::int FROM pending_invitations
                           WHERE company_id = @CompanyId
                             AND accepted_at IS NULL AND revoked_at IS NULL AND expires_at > NOW();
                           """;
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public Task<int> RevokeAsync(Guid companyId, Guid invitationId)
    {
        const string sql = """
                           UPDATE pending_invitations
                           SET revoked_at = NOW()
                           WHERE invitation_id = @InvitationId AND company_id = @CompanyId
                             AND accepted_at IS NULL AND revoked_at IS NULL;
                           """;
        return _db.ExecuteAsync(sql, new { CompanyId = companyId, InvitationId = invitationId });
    }
}
