using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class AccountGateway(IAccountRepository accountRepository) : IAccountGateway
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public Task AddAccount(Account account) => _accountRepository.AddAccount(account.ToRow());

    public Task<bool> ExistByEmailAsync(string normalizedEmail) => _accountRepository.ExistByEmailAsync(normalizedEmail);

    public async Task<Account?> GetByIdAsync(Guid accountId)
    {
        var row = await _accountRepository.GetByIdAsync(accountId);
        return row?.ToDomain();
    }

    public Task SetEmailConfirmedAsync(Guid accountId) => _accountRepository.SetEmailConfirmedAsync(accountId);
}
