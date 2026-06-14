namespace TKT.Infrastructure.Abstractions;

public interface ITenantContext
{
    Guid? AccountId { get; set; }
    Guid? CompanyId { get; set; }
}
