using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IAccountRepository
{
    Task AddAccount(AccountRow accountRow);
    Task<bool> ExistByEmailAsync(string normalizedEmail);
    Task<AccountRow?> GetByIdAsync(Guid accountId);
    Task SetEmailConfirmedAsync(Guid accountId);
}
