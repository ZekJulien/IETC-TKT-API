using TKT.Core.Domain;

namespace TKT.Core.Domain.Entities;

public class PendingInvitation
{
    public Guid InvitationId { get; set; }
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = CompanyRoles.Member;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public Guid InvitedBy { get; set; }
}
