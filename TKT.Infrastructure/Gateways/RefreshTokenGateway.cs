using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class RefreshTokenGateway(IRefreshTokenRepository repository) : IRefreshTokenGateway
{
    private readonly IRefreshTokenRepository _repository = repository;

    public Task AddAsync(RefreshToken token) => _repository.AddAsync(token.ToRow());
}
