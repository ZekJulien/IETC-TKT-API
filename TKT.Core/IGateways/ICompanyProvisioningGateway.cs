using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ICompanyProvisioningGateway
{
    Task<bool> ExistsByNameOrSlugAsync(string companyName, string companySlug);
    Task AddCompanyAsync(Company company);
    Task AddFreeSubscriptionAsync(Guid companyId);
}
