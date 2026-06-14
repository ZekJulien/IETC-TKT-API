using System.Data;

namespace TKT.Infrastructure.Persistence;

public interface ISystemDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}
