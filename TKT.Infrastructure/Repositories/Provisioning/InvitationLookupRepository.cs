using TKT.Infrastructure.Persistence.Abstractions;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories.Provisioning;

public class InvitationLookupRepository(ISystemDbSession db) : IInvitationLookupRepository
{
    private readonly ISystemDbSession _db = db;

    public Task<PendingInvitationRow?> GetActiveByCodeAsync(string invitationCode)
    {
        const string sql = """
                           SELECT invitation_id, company_id, email, role, department, job_title, invited_by
                           FROM pending_invitations
                           WHERE invitation_code = @InvitationCode
                             AND accepted_at IS NULL
                             AND revoked_at IS NULL
                             AND expires_at > NOW();
                           """;
        return _db.QuerySingleOrDefaultAsync<PendingInvitationRow>(sql, new { InvitationCode = invitationCode });
    }

    public Task MarkAcceptedAsync(Guid invitationId, Guid acceptedBy)
    {
        const string sql = """
                           UPDATE pending_invitations
                           SET accepted_at = NOW(), accepted_by = @AcceptedBy
                           WHERE invitation_id = @InvitationId;
                           """;
        return _db.ExecuteAsync(sql, new { InvitationId = invitationId, AcceptedBy = acceptedBy });
    }
}
