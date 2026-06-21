using System.Data;

namespace TKT.Infrastructure.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}
