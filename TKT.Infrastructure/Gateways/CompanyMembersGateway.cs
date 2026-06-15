using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class CompanyMembersGateway(ICompanyMembersRepository repository) : ICompanyMembersGateway
{
    private readonly ICompanyMembersRepository _repository = repository;

    public Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId)
        => _repository.GetActiveRoleAsync(companyId, accountId);

    public Task<bool> MemberExistsAsync(Guid companyId, Guid accountId)
        => _repository.MemberExistsAsync(companyId, accountId);

    public Task AddMemberAsync(CompanyMember member)
        => _repository.AddMemberAsync(member.ToRow());

    public Task<int> CountActiveMembersAsync(Guid companyId)
        => _repository.CountActiveMembersAsync(companyId);
}
