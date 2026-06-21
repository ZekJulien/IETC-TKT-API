using System.Data;
using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Infrastructure.Persistence;

public sealed class SystemDbSession(ISystemDbConnectionFactory factory, IRequestContext requestContext, ITenantContext tenantContext)
    : DbSessionBase(requestContext, tenantContext), ISystemDbSession
{
    protected override Task<IDbConnection> OpenConnectionAsync(CancellationToken ct)
        => factory.CreateOpenConnectionAsync(ct);
}
