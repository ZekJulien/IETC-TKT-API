namespace TKT.Infrastructure.Models;

public sealed class MemberSummaryRow
{
    public Guid? AccountId { get; set; }
    public Guid? InvitationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? JoinedAt { get; set; }
}
