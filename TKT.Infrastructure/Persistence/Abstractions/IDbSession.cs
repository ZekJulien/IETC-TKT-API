namespace TKT.Infrastructure.Persistence.Abstractions;

public interface IDbSession
{
    Task<int> ExecuteAsync(string sql, object? param = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? param = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null) where T : class;
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null);
}
