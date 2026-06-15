namespace TKT.Infrastructure.Models;

public sealed class MemberCompanyRow
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanySlug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
