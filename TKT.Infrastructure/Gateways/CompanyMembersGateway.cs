using TKT.Core.Common;
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

    public async Task<CompanyMember?> GetMemberAsync(Guid companyId, Guid accountId)
    {
        var row = await _repository.GetMemberAsync(companyId, accountId);
        return row?.ToDomain();
    }

    public Task<int> CountActiveOwnersAsync(Guid companyId)
        => _repository.CountActiveOwnersAsync(companyId);

    public Task UpdateRoleAsync(Guid companyId, Guid accountId, string role)
        => _repository.UpdateRoleAsync(companyId, accountId, role);

    public Task SetActiveAsync(Guid companyId, Guid accountId, bool isActive)
        => _repository.SetActiveAsync(companyId, accountId, isActive);

    public async Task<PagedResult<MemberSummary>> ListAsync(Guid companyId, int page, int pageSize, string? role, bool? isActive)
    {
        var rows = await _repository.ListAsync(companyId, page, pageSize, role, isActive);
        var total = await _repository.CountAsync(companyId, role, isActive);
        return new PagedResult<MemberSummary>(rows.Select(r => r.ToSummary()).ToList(), total, page, pageSize);
    }

    public async Task<IReadOnlyList<MemberDirectoryEntry>> ListDirectoryAsync(Guid companyId)
    {
        var rows = await _repository.ListDirectoryAsync(companyId);
        return rows.Select(r => r.ToDirectoryEntry()).ToList();
    }
}
