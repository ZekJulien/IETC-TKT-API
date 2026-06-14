namespace TKT.Infrastructure.Models;

public sealed class CompanyRow
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanySlug { get; set; } = string.Empty;
}
