using TKT.Infrastructure.Persistence.Abstractions;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories.Provisioning;

public class CompanyProvisioningRepository(ISystemDbSession db) : ICompanyProvisioningRepository
{
    private readonly ISystemDbSession _db = db;

    public Task<bool> ExistsByNameOrSlugAsync(string companyName, string companySlug)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM companies WHERE company_name = @CompanyName OR company_slug = @CompanySlug)";
        return _db.ExecuteScalarAsync<bool>(sql, new { CompanyName = companyName, CompanySlug = companySlug });
    }

    public Task AddCompanyAsync(CompanyRow company)
    {
        const string sql = """
                           INSERT INTO companies (company_id, company_name, company_slug)
                           VALUES (@CompanyId, @CompanyName, @CompanySlug);
                           """;
        return _db.ExecuteAsync(sql, company);
    }

    public Task AddFreeSubscriptionAsync(Guid companyId)
    {
        const string sql = "INSERT INTO company_subscriptions (company_id) VALUES (@CompanyId);";
        return _db.ExecuteAsync(sql, new { CompanyId = companyId });
    }
}
