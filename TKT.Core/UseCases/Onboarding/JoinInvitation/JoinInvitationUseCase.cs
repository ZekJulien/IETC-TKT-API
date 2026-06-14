using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Onboarding.JoinInvitation;

public sealed class JoinInvitationUseCase(
    IInvitationGateway invitationGateway,
    ICompanyMemberProvisioningGateway memberGateway) : IJoinInvitationUseCase
{
    private readonly IInvitationGateway _invitationGateway = invitationGateway;
    private readonly ICompanyMemberProvisioningGateway _memberGateway = memberGateway;

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

        return new JoinInvitationResult(invitation.CompanyId, invitation.Role);
    }
}
