using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class AccountGateway(IAccountRepository accountRepository) : IAccountGateway
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public Task AddAccount(Account account)
    {
        return _accountRepository.AddAccount(account.ToRow());
    }

    public Task<bool> ExistByEmailAsync(string normalizedEmail)
    {
        return _accountRepository.ExistByEmailAsync(normalizedEmail);
    }
}