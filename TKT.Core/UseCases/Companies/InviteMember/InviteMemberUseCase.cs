using System.Security.Cryptography;
using TKT.Core.Abstractions;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.InviteMember;

public sealed class InviteMemberUseCase(
    ICompanyMembersGateway members,
    ICompanyInvitationGateway invitations,
    ICompanySubscriptionGateway subscriptions,
    IAccountGateway accounts,
    IInvitationNotifier notifier) : IInviteMemberUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICompanyInvitationGateway _invitations = invitations;
    private readonly ICompanySubscriptionGateway _subscriptions = subscriptions;
    private readonly IAccountGateway _accounts = accounts;
    private readonly IInvitationNotifier _notifier = notifier;

    public async Task<InviteMemberResult> ExecuteAsync(InviteMemberInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(InvitationErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.InviteMember))
            throw new ForbiddenException(InvitationErrors.Forbidden);

        var email = Email.Create(input.Email);
        var role = MemberRole.CreateInvitable(input.Role);

        var maxUsers = await _subscriptions.GetMaxUsersAsync(input.CompanyId);
        var activeMembers = await _members.CountActiveMembersAsync(input.CompanyId);
        if (activeMembers >= maxUsers)
            throw new ConflictException(InvitationErrors.QuotaExceeded);

        var account = await _accounts.GetByNormalizedEmailAsync(email.Normalized);
        if (account is not null)
        {
            if (await _members.MemberExistsAsync(input.CompanyId, account.AccountId))
                throw new ConflictException(InvitationErrors.AlreadyMember);

            var member = new CompanyMember
            {
                MembershipId = Guid.CreateVersion7(),
                CompanyId = input.CompanyId,
                AccountId = account.AccountId,
                Role = role.Value,
                InvitedBy = input.CallerAccountId,
                Department = input.Department,
                JobTitle = input.JobTitle,
                JoinedAt = DateTimeOffset.UtcNow,
            };
            await _members.AddMemberAsync(member);
            await _notifier.NotifyMemberAddedAsync(input.CompanyId, email.Value, member.MembershipId);

            return new InviteMemberResult(InviteMode.DirectMember, role.Value);
        }

        if (await _invitations.HasActivePendingAsync(input.CompanyId, email.Value))
            throw new ConflictException(InvitationErrors.AlreadyInvited);

        var invitation = new PendingInvitation
        {
            InvitationId = Guid.CreateVersion7(),
            CompanyId = input.CompanyId,
            Email = email.Value,
            Role = role.Value,
            Department = input.Department,
            JobTitle = input.JobTitle,
            InvitedBy = input.CallerAccountId,
            InvitationCode = GenerateInvitationCode(),
        };
        await _invitations.CreateAsync(invitation);
        await _notifier.NotifyInvitationAsync(input.CompanyId, email.Value, invitation.InvitationId, invitation.InvitationCode);

        return new InviteMemberResult(InviteMode.PendingInvitation, role.Value);
    }

    private static string GenerateInvitationCode()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}
