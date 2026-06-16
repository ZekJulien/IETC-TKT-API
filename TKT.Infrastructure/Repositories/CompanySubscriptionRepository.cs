using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class CompanySubscriptionRepository(IDbSession db) : ICompanySubscriptionRepository
{
    private readonly IDbSession _db = db;

    public Task<int> GetMaxUsersAsync(Guid companyId)
    {
        const string sql = """
                           SELECT max_users
                           FROM company_subscriptions
                           WHERE company_id = @CompanyId AND valid_during @> NOW()
                           LIMIT 1;
                           """;
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public Task<int> GetMaxTicketsPerMonthAsync(Guid companyId)
    {
        const string sql = """
                           SELECT max_tickets_per_month
                           FROM company_subscriptions
                           WHERE company_id = @CompanyId AND valid_during @> NOW()
                           LIMIT 1;
                           """;
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }
}
