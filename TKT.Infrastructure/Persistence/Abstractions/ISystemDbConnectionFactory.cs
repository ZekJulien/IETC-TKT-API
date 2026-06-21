using System.Data;

namespace TKT.Infrastructure.Persistence.Abstractions;

public interface ISystemDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}
