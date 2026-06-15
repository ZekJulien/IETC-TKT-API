using TKT.Core.Domain.Entities;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class PendingInvitationMapper
{
    public static PendingInvitation ToDomain(this PendingInvitationRow row)
    {
        return new PendingInvitation
        {
            InvitationId = row.InvitationId,
            CompanyId = row.CompanyId,
            Email = row.Email,
            Role = row.Role,
            Department = row.Department,
            JobTitle = row.JobTitle,
            InvitationCode = row.InvitationCode,
            InvitedBy = row.InvitedBy,
        };
    }

    public static PendingInvitationRow ToRow(this PendingInvitation invitation)
    {
        return new PendingInvitationRow
        {
            InvitationId = invitation.InvitationId,
            CompanyId = invitation.CompanyId,
            Email = invitation.Email,
            Role = invitation.Role,
            Department = invitation.Department,
            JobTitle = invitation.JobTitle,
            InvitationCode = invitation.InvitationCode,
            InvitedBy = invitation.InvitedBy,
        };
    }
}
