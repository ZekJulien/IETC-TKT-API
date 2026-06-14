namespace TKT.Core.Domain.Entities;

public class Account
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? SecurityStamp { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}