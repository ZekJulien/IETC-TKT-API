using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class MembershipGateway(IMembershipReadRepository repository) : IMembershipGateway
{
    private readonly IMembershipReadRepository _repository = repository;

    public async Task<IReadOnlyList<CompanyMember>> GetActiveForAccountAsync(Guid accountId)
    {
        var rows = await _repository.GetActiveForAccountAsync(accountId);
        return rows.Select(r => r.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<MemberCompany>> GetCompaniesForAccountAsync(Guid accountId)
    {
        var rows = await _repository.GetCompaniesForAccountAsync(accountId);
        return rows.Select(r => r.ToMemberCompany()).ToList();
    }
}
