using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class CompanyInvitationGateway(ICompanyInvitationRepository repository) : ICompanyInvitationGateway
{
    private readonly ICompanyInvitationRepository _repository = repository;

    public Task<bool> HasActivePendingAsync(Guid companyId, string email)
        => _repository.HasActivePendingAsync(companyId, email);

    public Task CreateAsync(PendingInvitation invitation)
        => _repository.CreateAsync(invitation.ToRow());
}
