using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class RefreshTokenGateway(IRefreshTokenRepository repository, IRefreshTokenReuseRepository reuseRepository)
    : IRefreshTokenGateway
{
    private readonly IRefreshTokenRepository _repository = repository;
    private readonly IRefreshTokenReuseRepository _reuseRepository = reuseRepository;

    public Task AddAsync(RefreshToken token) => _repository.AddAsync(token.ToRow());

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        var row = await _repository.GetByHashAsync(tokenHash);
        return row?.ToDomain();
    }

    public Task MarkRotatedAsync(Guid tokenId, Guid replacedById)
        => _repository.MarkRotatedAsync(tokenId, replacedById);

    public Task RevokeFamilyForReuseAsync(Guid familyId, Guid accountId)
        => _reuseRepository.RevokeFamilyAsync(familyId, accountId);
}
