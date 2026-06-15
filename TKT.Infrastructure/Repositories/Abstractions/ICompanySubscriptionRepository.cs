namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICompanySubscriptionRepository
{
    Task<int> GetMaxUsersAsync(Guid companyId);
}
