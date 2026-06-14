using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface IInvitationGateway
{
    Task<PendingInvitation?> GetActiveByCodeAsync(string invitationCode);
    Task MarkAcceptedAsync(Guid invitationId, Guid acceptedBy);
}
