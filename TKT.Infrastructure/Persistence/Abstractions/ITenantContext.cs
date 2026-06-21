namespace TKT.Infrastructure.Persistence.Abstractions;

public interface ITenantContext
{
    Guid? AccountId { get; set; }
    Guid? CompanyId { get; set; }
}
