using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IMembershipReadRepository
{
    Task<IReadOnlyList<CompanyMemberRow>> GetActiveForAccountAsync(Guid accountId);
}
