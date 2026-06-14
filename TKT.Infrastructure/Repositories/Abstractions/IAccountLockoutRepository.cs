namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IAccountLockoutRepository
{
    Task RegisterFailedLoginAsync(Guid accountId, int failedCount, DateTimeOffset? lockoutEnd);
}
