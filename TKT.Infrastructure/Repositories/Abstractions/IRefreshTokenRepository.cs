using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshTokenRow row);
    Task<RefreshTokenRow?> GetByHashAsync(string tokenHash);
    Task MarkRotatedAsync(Guid tokenId, Guid replacedById);
}
