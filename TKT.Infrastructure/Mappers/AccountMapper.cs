using TKT.Core.Domain.Entities;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class AccountMapper
{
    public static Account ToDomain(this AccountRow account)
    {
        return new Account
        {
            AccountId = account.AccountId,
            Email = account.Email,
            NormalizedEmail = account.NormalizedEmail,
            PasswordHash = account.PasswordHash,
            SecurityStamp = account.SecurityStamp,
            EmailConfirmed = account.EmailConfirmed,
            IsActive = account.IsActive,
            AccessFailedCount = account.AccessFailedCount,
            LockoutEnd = account.LockoutEnd,
        };
    }

    public static AccountRow ToRow(this Account account)
    {
        return new AccountRow
        {
            AccountId = account.AccountId,
            Email = account.Email,
            NormalizedEmail = account.NormalizedEmail,
            PasswordHash = account.PasswordHash,
            SecurityStamp = account.SecurityStamp,
        };
    }
}
