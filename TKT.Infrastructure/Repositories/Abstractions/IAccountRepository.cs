using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IAccountRepository
{
    Task AddAccount(AccountRow accountRow);
    Task<bool> ExistByEmailAsync(string normalizedEmail);
    Task<AccountRow?> GetByIdAsync(Guid accountId);
    Task<AccountRow?> GetByNormalizedEmailAsync(string normalizedEmail);
    Task SetEmailConfirmedAsync(Guid accountId);
    Task ResetLockoutAsync(Guid accountId);
}
