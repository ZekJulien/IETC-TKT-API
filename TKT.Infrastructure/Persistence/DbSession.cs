using System.Data;
using System.Data.Common;

namespace TKT.Infrastructure.Persistence;

public interface IDbSession
{
    Task<IDbConnection> GetConnectionAsync(CancellationToken ct = default);
    IDbTransaction? Transaction { get; }
}

public sealed class DbSession : IDbSession, IAsyncDisposable
{
    private readonly IDbConnectionFactory _factory;
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    public DbSession(IDbConnectionFactory factory) => _factory = factory;

    public IDbTransaction? Transaction => _transaction;

    public async Task<IDbConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is null)
        {
            _connection = (DbConnection)await _factory.CreateOpenConnectionAsync(ct);
            _transaction = await _connection.BeginTransactionAsync(ct);
        }
        return _connection;
    }

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
