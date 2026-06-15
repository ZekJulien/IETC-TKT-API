using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Identity.ListMyCompanies;

public sealed class ListMyCompaniesUseCase(IMembershipGateway membershipGateway) : IListMyCompaniesUseCase
{
    private readonly IMembershipGateway _membershipGateway = membershipGateway;

    public async Task<ListMyCompaniesResult> ExecuteAsync(ListMyCompaniesInput input)
    {
        var companies = await _membershipGateway.GetCompaniesForAccountAsync(input.AccountId);
        return new ListMyCompaniesResult(companies);
    }
}
