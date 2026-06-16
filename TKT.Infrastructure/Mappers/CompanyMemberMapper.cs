using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
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
            IsActive = member.IsActive,
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
            IsActive = row.IsActive,
            InvitedBy = row.InvitedBy,
            Department = row.Department,
            JobTitle = row.JobTitle,
            JoinedAt = row.JoinedAt,
        };
    }

    public static MemberSummary ToSummary(this MemberSummaryRow row)
        => new(row.AccountId, row.InvitationId, row.Email, row.DisplayName, row.Role, row.Status, row.JoinedAt);

    public static MemberDirectoryEntry ToDirectoryEntry(this MemberDirectoryRow row)
        => new(row.AccountId, row.Email, row.Role, row.FirstName, row.LastName);

    public static MemberCompany ToMemberCompany(this MemberCompanyRow row)
        => new(row.CompanyId, row.CompanyName, row.CompanySlug, row.LogoUrl, row.Role, row.IsActive);
}
