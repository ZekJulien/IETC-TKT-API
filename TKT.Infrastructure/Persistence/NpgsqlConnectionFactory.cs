using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TKT.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
        => await _dataSource.OpenConnectionAsync(ct);

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}
