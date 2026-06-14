namespace TKT.Infrastructure.Models;

public sealed class PendingInvitationRow
{
    public Guid InvitationId { get; set; }
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public Guid InvitedBy { get; set; }
}
