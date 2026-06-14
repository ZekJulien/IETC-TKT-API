namespace TKT.Infrastructure.Models;

public sealed class CompanyMemberRow
{
    public Guid MembershipId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? InvitedBy { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTimeOffset? JoinedAt { get; set; }
}
