using TKT.Core.Common;
using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ICompanyMembersGateway
{
    Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId);
    Task<bool> MemberExistsAsync(Guid companyId, Guid accountId);
    Task AddMemberAsync(CompanyMember member);
    Task<int> CountActiveMembersAsync(Guid companyId);
    Task<CompanyMember?> GetMemberAsync(Guid companyId, Guid accountId);
    Task<int> CountActiveOwnersAsync(Guid companyId);
    Task UpdateRoleAsync(Guid companyId, Guid accountId, string role);
    Task SetActiveAsync(Guid companyId, Guid accountId, bool isActive);
    Task<PagedResult<MemberSummary>> ListAsync(Guid companyId, int page, int pageSize, string? role, bool? isActive);
    Task<IReadOnlyList<MemberDirectoryEntry>> ListDirectoryAsync(Guid companyId);
}
