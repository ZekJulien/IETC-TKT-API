using TKT.Core.IGateways;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class CompanySubscriptionGateway(ICompanySubscriptionRepository repository) : ICompanySubscriptionGateway
{
    private readonly ICompanySubscriptionRepository _repository = repository;

    public Task<int> GetMaxUsersAsync(Guid companyId)
        => _repository.GetMaxUsersAsync(companyId);
}
