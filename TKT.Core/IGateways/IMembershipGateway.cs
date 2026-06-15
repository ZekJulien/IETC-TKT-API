using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IMembershipGateway
{
    Task<IReadOnlyList<CompanyMember>> GetActiveForAccountAsync(Guid accountId);

    Task<IReadOnlyList<MemberCompany>> GetCompaniesForAccountAsync(Guid accountId);
}
