using System.Data;
using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Infrastructure.Persistence;

public sealed class DbSession(IDbConnectionFactory factory, IRequestContext requestContext, ITenantContext tenantContext)
    : DbSessionBase(requestContext, tenantContext), IDbSession
{
    protected override Task<IDbConnection> OpenConnectionAsync(CancellationToken ct)
        => factory.CreateOpenConnectionAsync(ct);
}
