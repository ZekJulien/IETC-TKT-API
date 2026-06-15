using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICompanyMembersRepository
{
    Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId);
    Task<bool> MemberExistsAsync(Guid companyId, Guid accountId);
    Task AddMemberAsync(CompanyMemberRow member);
    Task<int> CountActiveMembersAsync(Guid companyId);
}
