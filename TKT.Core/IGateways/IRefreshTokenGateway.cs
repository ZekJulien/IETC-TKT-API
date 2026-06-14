using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IRefreshTokenGateway
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task MarkRotatedAsync(Guid tokenId, Guid replacedById);
    Task RevokeFamilyForReuseAsync(Guid familyId, Guid accountId);
}
