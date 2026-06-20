using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class ProfileGateway(IProfileRepository repository) : IProfileGateway
{
    private readonly IProfileRepository _repository = repository;

    public Task AddAsync(UserProfile profile) => _repository.AddAsync(profile.ToRow());

    public async Task<UserProfile?> GetByAccountIdAsync(Guid accountId)
    {
        var row = await _repository.GetByAccountIdAsync(accountId);
        return row?.ToDomain();
    }
}
