using System.Data;
using System.Data.Common;
using TKT.Infrastructure.Abstractions;

namespace TKT.Infrastructure.Persistence;

public interface IDbSession
{
    Task<IDbConnection> GetConnectionAsync();
    IDbTransaction? Transaction { get; }
}

public sealed class DbSession(IDbConnectionFactory factory, IRequestContext requestContext)
    : IDbSession, IAsyncDisposable
{
    private readonly IRequestContext _requestContext = requestContext;
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    public IDbTransaction? Transaction => _transaction;

    public async Task<IDbConnection> GetConnectionAsync()
    {
        if (_connection is null)
        {
            _connection = (DbConnection)await factory.CreateOpenConnectionAsync(_requestContext.RequestAborted);
            _transaction = await _connection.BeginTransactionAsync(_requestContext.RequestAborted);
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
