using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ICompanyMemberProvisioningGateway
{
    Task<bool> MemberExistsAsync(Guid companyId, Guid accountId);
    Task AddMemberAsync(CompanyMember member);
}
