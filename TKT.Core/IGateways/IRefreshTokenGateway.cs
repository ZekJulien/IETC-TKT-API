using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IRefreshTokenGateway
{
    Task AddAsync(RefreshToken token);
}
