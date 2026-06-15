using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ICompanyMembersGateway
{
    Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId);
    Task<bool> MemberExistsAsync(Guid companyId, Guid accountId);
    Task AddMemberAsync(CompanyMember member);
    Task<int> CountActiveMembersAsync(Guid companyId);
}
