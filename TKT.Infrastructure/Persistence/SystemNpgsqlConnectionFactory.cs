using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TKT.Infrastructure.Persistence;

public sealed class SystemNpgsqlConnectionFactory : ISystemDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public SystemNpgsqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SystemConnection")
            ?? throw new InvalidOperationException("Connection string 'SystemConnection' is not configured.");

        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
        => await _dataSource.OpenConnectionAsync(ct);

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}
