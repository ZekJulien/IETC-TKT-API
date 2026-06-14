namespace TKT.Core.Domain.Entities;

public class Company
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
