namespace TKT.Infrastructure.Models;

public sealed class MemberSummaryRow
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset? JoinedAt { get; set; }
}
