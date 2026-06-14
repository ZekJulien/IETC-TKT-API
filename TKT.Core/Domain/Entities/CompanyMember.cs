using TKT.Core.Domain;

namespace TKT.Core.Domain.Entities;

public class CompanyMember
{
    public Guid MembershipId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public string Role { get; set; } = CompanyRoles.Member;
    public Guid? InvitedBy { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTimeOffset? JoinedAt { get; set; }
}
