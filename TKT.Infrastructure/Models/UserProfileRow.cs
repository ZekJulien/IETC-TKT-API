namespace TKT.Infrastructure.Models;

public sealed class UserProfileRow
{
    public Guid AccountId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
