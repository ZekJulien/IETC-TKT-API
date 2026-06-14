using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class CompanyMemberProvisioningGateway(ICompanyMemberProvisioningRepository repository) : ICompanyMemberProvisioningGateway
{
    private readonly ICompanyMemberProvisioningRepository _repository = repository;

    public Task<bool> MemberExistsAsync(Guid companyId, Guid accountId)
        => _repository.MemberExistsAsync(companyId, accountId);

    public Task AddMemberAsync(CompanyMember member) => _repository.AddMemberAsync(member.ToRow());
}
