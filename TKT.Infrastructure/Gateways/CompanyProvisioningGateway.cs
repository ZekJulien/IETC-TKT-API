using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class CompanyProvisioningGateway(ICompanyProvisioningRepository repository) : ICompanyProvisioningGateway
{
    private readonly ICompanyProvisioningRepository _repository = repository;

    public Task<bool> ExistsByNameOrSlugAsync(string companyName, string companySlug)
        => _repository.ExistsByNameOrSlugAsync(companyName, companySlug);

    public Task AddCompanyAsync(Company company) => _repository.AddCompanyAsync(company.ToRow());

    public Task AddFreeSubscriptionAsync(Guid companyId) => _repository.AddFreeSubscriptionAsync(companyId);
}
