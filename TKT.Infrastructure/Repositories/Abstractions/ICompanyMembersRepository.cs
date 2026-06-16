using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICompanyMembersRepository
{
    Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId);
    Task<bool> MemberExistsAsync(Guid companyId, Guid accountId);
    Task AddMemberAsync(CompanyMemberRow member);
    Task<int> CountActiveMembersAsync(Guid companyId);
    Task<CompanyMemberRow?> GetMemberAsync(Guid companyId, Guid accountId);
    Task<int> CountActiveOwnersAsync(Guid companyId);
    Task UpdateRoleAsync(Guid companyId, Guid accountId, string role);
    Task SetActiveAsync(Guid companyId, Guid accountId, bool isActive);
    Task<IReadOnlyList<MemberSummaryRow>> ListAsync(Guid companyId, int page, int pageSize, string? role, bool? isActive);
    Task<int> CountAsync(Guid companyId, string? role, bool? isActive);
    Task<IReadOnlyList<MemberDirectoryRow>> ListDirectoryAsync(Guid companyId);
}
