namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICompanySubscriptionRepository
{
    Task<int> GetMaxUsersAsync(Guid companyId);
    Task<int> GetMaxTicketsPerMonthAsync(Guid companyId);
}
