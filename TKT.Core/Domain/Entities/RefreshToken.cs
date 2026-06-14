namespace TKT.Core.Domain.Entities;

public class RefreshToken
{
    public Guid TokenId { get; set; }
    public Guid AccountId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset AbsoluteExpiresAt { get; set; }
}
