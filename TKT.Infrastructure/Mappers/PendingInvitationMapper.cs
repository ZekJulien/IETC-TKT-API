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
            InvitedBy = row.InvitedBy,
        };
    }
}
