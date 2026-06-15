using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICompanyInvitationRepository
{
    Task<bool> HasActivePendingAsync(Guid companyId, string email);
    Task CreateAsync(PendingInvitationRow invitation);
    Task<int> CountActivePendingAsync(Guid companyId);
    Task<int> RevokeAsync(Guid companyId, Guid invitationId);
}
