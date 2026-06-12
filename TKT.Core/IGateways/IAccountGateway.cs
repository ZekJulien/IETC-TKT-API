using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IAccountGateway
{
    Task AddAccount(Account account);
    Task<bool> ExistByEmailAsync(string normalizedEmail);
}