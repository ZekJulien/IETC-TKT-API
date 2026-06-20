using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IProfileRepository
{
    Task AddAsync(UserProfileRow profile);
    Task<UserProfileRow?> GetByAccountIdAsync(Guid accountId);
}
