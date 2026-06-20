using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Onboarding.JoinInvitation;

public sealed class JoinInvitationUseCase(
    IInvitationGateway invitationGateway,
    ICompanyMemberProvisioningGateway memberGateway,
    ITokenService tokenService,
    IRefreshTokenIssuer refreshTokenIssuer) : IJoinInvitationUseCase
{
    private readonly IInvitationGateway _invitationGateway = invitationGateway;
    private readonly ICompanyMemberProvisioningGateway _memberGateway = memberGateway;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IRefreshTokenIssuer _refreshTokenIssuer = refreshTokenIssuer;

    public async Task<JoinInvitationResult> ExecuteAsync(JoinInvitationInput input)
    {
        var invitation = await _invitationGateway.GetActiveByCodeAsync(input.InvitationCode)
            ?? throw new NotFoundException(InvitationErrors.NotFound);

        if (!string.Equals(invitation.Email, input.Email, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(InvitationErrors.EmailMismatch);

        if (await _memberGateway.MemberExistsAsync(invitation.CompanyId, input.AccountId))
            throw new ConflictException(InvitationErrors.AlreadyMember);

        var member = new CompanyMember
        {
            MembershipId = Guid.CreateVersion7(),
            CompanyId = invitation.CompanyId,
            AccountId = input.AccountId,
            Role = invitation.Role,
            InvitedBy = invitation.InvitedBy,
            Department = invitation.Department,
            JobTitle = invitation.JobTitle,
            JoinedAt = DateTimeOffset.UtcNow,
        };
        await _memberGateway.AddMemberAsync(member);
        await _invitationGateway.MarkAcceptedAsync(invitation.InvitationId, input.AccountId);

        var accessToken = _tokenService.GenerateAccessToken(input.AccountId, input.Email, invitation.CompanyId, invitation.Role);
        var refreshToken = await _refreshTokenIssuer.IssueAsync(input.AccountId, invitation.CompanyId);

        return new JoinInvitationResult(invitation.CompanyId, invitation.Role, accessToken, refreshToken);
    }
}
