using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICompanyProvisioningRepository
{
    Task<bool> ExistsByNameOrSlugAsync(string companyName, string companySlug);
    Task AddCompanyAsync(CompanyRow company);
    Task AddFreeSubscriptionAsync(Guid companyId);
}
