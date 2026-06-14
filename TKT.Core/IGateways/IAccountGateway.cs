using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IAccountGateway
{
    Task AddAccount(Account account);
    Task<bool> ExistByEmailAsync(string normalizedEmail);
    Task<Account?> GetByIdAsync(Guid accountId);
    Task<Account?> GetByNormalizedEmailAsync(string normalizedEmail);
    Task SetEmailConfirmedAsync(Guid accountId);
    Task RegisterFailedLoginAsync(Guid accountId, int failedCount, DateTimeOffset? lockoutEnd);
    Task ResetLockoutAsync(Guid accountId);
}
