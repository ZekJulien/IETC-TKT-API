using TKT.Infrastructure.Abstractions;

namespace TKT.Infrastructure.Persistence;

public sealed class TenantContext : ITenantContext
{
    public Guid? AccountId { get; set; }
    public Guid? CompanyId { get; set; }
}
