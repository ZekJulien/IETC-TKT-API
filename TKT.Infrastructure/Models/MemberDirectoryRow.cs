namespace TKT.Infrastructure.Models;

public sealed class MemberDirectoryRow
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
