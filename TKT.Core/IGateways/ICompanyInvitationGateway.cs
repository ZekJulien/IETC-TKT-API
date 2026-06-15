using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ICompanyInvitationGateway
{
    Task<bool> HasActivePendingAsync(Guid companyId, string email);
    Task CreateAsync(PendingInvitation invitation);
}
