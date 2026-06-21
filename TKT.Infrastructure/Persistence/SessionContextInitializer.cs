using System.Data.Common;
using Dapper;
using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Infrastructure.Persistence;

internal static class SessionContextInitializer
{
    public static async Task ApplyAsync(DbConnection connection, DbTransaction transaction, ITenantContext tenantContext, CancellationToken ct)
    {
        if (tenantContext.AccountId is { } accountId)
            await SetConfigAsync(connection, transaction, "app.current_user_id", accountId, ct);
        if (tenantContext.CompanyId is { } companyId)
            await SetConfigAsync(connection, transaction, "app.current_company_id", companyId, ct);
    }

    private static Task SetConfigAsync(DbConnection connection, DbTransaction transaction, string key, Guid value, CancellationToken ct)
    {
        const string sql = "SELECT set_config(@key, @value, true)";
        return connection.ExecuteAsync(new CommandDefinition(sql,
            new { key, value = value.ToString() }, transaction, cancellationToken: ct));
    }
}
