using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.RevokeInvitation;

public sealed class RevokeInvitationUseCase(
    ICompanyMembersGateway members,
    ICompanyInvitationGateway invitations) : IRevokeInvitationUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICompanyInvitationGateway _invitations = invitations;

    public async Task ExecuteAsync(RevokeInvitationInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(InvitationErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.InviteMember))
            throw new ForbiddenException(InvitationErrors.Forbidden);

        var revoked = await _invitations.RevokeAsync(input.CompanyId, input.InvitationId);
        if (revoked == 0)
            throw new NotFoundException(InvitationErrors.NotFound);
    }
}
