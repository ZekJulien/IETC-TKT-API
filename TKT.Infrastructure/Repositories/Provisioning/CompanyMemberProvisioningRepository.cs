using TKT.Infrastructure.Persistence.Abstractions;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories.Provisioning;

public class CompanyMemberProvisioningRepository(ISystemDbSession db) : ICompanyMemberProvisioningRepository
{
    private readonly ISystemDbSession _db = db;

    public Task<bool> MemberExistsAsync(Guid companyId, Guid accountId)
        => _db.ExecuteScalarAsync<bool>(CompanyMemberSql.MemberExists, new { CompanyId = companyId, AccountId = accountId });

    public Task AddMemberAsync(CompanyMemberRow member)
        => _db.ExecuteAsync(CompanyMemberSql.Insert, member);
}
