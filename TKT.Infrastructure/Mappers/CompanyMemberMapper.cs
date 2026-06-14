using TKT.Core.Domain.Entities;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class CompanyMemberMapper
{
    public static CompanyMemberRow ToRow(this CompanyMember member)
    {
        return new CompanyMemberRow
        {
            MembershipId = member.MembershipId,
            CompanyId = member.CompanyId,
            AccountId = member.AccountId,
            Role = member.Role,
            InvitedBy = member.InvitedBy,
            Department = member.Department,
            JobTitle = member.JobTitle,
            JoinedAt = member.JoinedAt,
        };
    }

    public static CompanyMember ToDomain(this CompanyMemberRow row)
    {
        return new CompanyMember
        {
            MembershipId = row.MembershipId,
            CompanyId = row.CompanyId,
            AccountId = row.AccountId,
            Role = row.Role,
            InvitedBy = row.InvitedBy,
            Department = row.Department,
            JobTitle = row.JobTitle,
            JoinedAt = row.JoinedAt,
        };
    }
}
