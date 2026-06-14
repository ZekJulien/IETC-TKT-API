using System.Data;
using TKT.Infrastructure.Abstractions;

namespace TKT.Infrastructure.Persistence;

public interface IDbSession
{
    Task<int> ExecuteAsync(string sql, object? param = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? param = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null) where T : class;
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null);
}

public sealed class DbSession(IDbConnectionFactory factory, IRequestContext requestContext, ITenantContext tenantContext)
    : DbSessionBase(requestContext, tenantContext), IDbSession
{
    protected override Task<IDbConnection> OpenConnectionAsync(CancellationToken ct)
        => factory.CreateOpenConnectionAsync(ct);
}
