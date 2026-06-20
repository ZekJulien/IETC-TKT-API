namespace TKT.Core.Domain.Entities;

public class UserProfile
{
    public Guid AccountId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
