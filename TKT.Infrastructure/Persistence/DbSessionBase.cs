using System.Data;
using System.Data.Common;
using Dapper;
using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Infrastructure.Persistence;

public abstract class DbSessionBase(IRequestContext requestContext, ITenantContext tenantContext) : IAsyncDisposable
{
    private readonly IRequestContext _requestContext = requestContext;
    private readonly ITenantContext _tenantContext = tenantContext;
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    protected abstract Task<IDbConnection> OpenConnectionAsync(CancellationToken ct);

    public async Task<int> ExecuteAsync(string sql, object? param = null)
        => await (await ConnectionAsync()).ExecuteAsync(Command(sql, param));

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
        => (await (await ConnectionAsync()).ExecuteScalarAsync<T>(Command(sql, param)))!;

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null) where T : class
        => await (await ConnectionAsync()).QuerySingleOrDefaultAsync<T>(Command(sql, param));

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null)
        => (await (await ConnectionAsync()).QueryAsync<T>(Command(sql, param))).AsList();

    public async Task CommitAsync()
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync();
        await DisposeTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync();
        await DisposeTransactionAsync();
    }

    private async Task<DbConnection> ConnectionAsync()
    {
        if (_connection is null)
        {
            _connection = (DbConnection)await OpenConnectionAsync(_requestContext.RequestAborted);
            _transaction = await _connection.BeginTransactionAsync(_requestContext.RequestAborted);
            await SessionContextInitializer.ApplyAsync(_connection, _transaction, _tenantContext, _requestContext.RequestAborted);
        }
        return _connection;
    }

    private CommandDefinition Command(string sql, object? param)
        => new(sql, param, _transaction, cancellationToken: _requestContext.RequestAborted);

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
