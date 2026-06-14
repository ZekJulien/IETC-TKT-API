using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IInvitationLookupRepository
{
    Task<PendingInvitationRow?> GetActiveByCodeAsync(string invitationCode);
    Task MarkAcceptedAsync(Guid invitationId, Guid acceptedBy);
}
