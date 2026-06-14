using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class InvitationGateway(IInvitationLookupRepository repository) : IInvitationGateway
{
    private readonly IInvitationLookupRepository _repository = repository;

    public async Task<PendingInvitation?> GetActiveByCodeAsync(string invitationCode)
    {
        var row = await _repository.GetActiveByCodeAsync(invitationCode);
        return row?.ToDomain();
    }

    public Task MarkAcceptedAsync(Guid invitationId, Guid acceptedBy)
        => _repository.MarkAcceptedAsync(invitationId, acceptedBy);
}
