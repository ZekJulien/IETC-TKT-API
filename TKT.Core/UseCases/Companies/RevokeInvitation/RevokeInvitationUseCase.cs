using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Companies.RevokeInvitation;

public sealed class RevokeInvitationUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICompanyInvitationGateway invitations) : IRevokeInvitationUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICompanyInvitationGateway _invitations = invitations;

    public async Task ExecuteAsync(RevokeInvitationInput input)
    {
        var callerRole = await _authorizer.ResolveForCompanyAsync(input.CallerCompanyId, input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.InviteMember))
            throw new ForbiddenException(InvitationErrors.Forbidden);

        var revoked = await _invitations.RevokeAsync(input.CompanyId, input.InvitationId);
        if (revoked == 0)
            throw new NotFoundException(InvitationErrors.NotFound);
    }
}
