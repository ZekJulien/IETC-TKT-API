using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshTokenRow row);
}
