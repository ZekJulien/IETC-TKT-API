using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IProfileGateway
{
    Task AddAsync(UserProfile profile);
    Task<UserProfile?> GetByAccountIdAsync(Guid accountId);
}
