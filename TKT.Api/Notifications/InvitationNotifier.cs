using Microsoft.Extensions.Localization;
using TKT.Api.Email;
using TKT.Core.Abstractions;
using TKT.Core.IGateways;

namespace TKT.Api.Notifications;

public sealed class InvitationNotifier(
    IStringLocalizer<InvitationEmailMessages> localizer,
    IEmailQueueGateway emailQueue,
    IEmailSender emailSender) : IInvitationNotifier
{
    private readonly IStringLocalizer<InvitationEmailMessages> _localizer = localizer;
    private readonly IEmailQueueGateway _emailQueue = emailQueue;
    private readonly IEmailSender _emailSender = emailSender;

    public Task NotifyMemberAddedAsync(Guid companyId, string recipientEmail, Guid membershipId)
        => DispatchAsync(companyId, recipientEmail, membershipId, "company_member", "member_added",
            _localizer["email.member_added.subject"].Value, _localizer["email.member_added.body"].Value);

    public Task NotifyInvitationAsync(Guid companyId, string recipientEmail, Guid invitationId, string invitationCode)
        => DispatchAsync(companyId, recipientEmail, invitationId, "pending_invitation", "member_invitation",
            _localizer["email.member_invitation.subject"].Value, _localizer["email.member_invitation.body", invitationCode].Value);

    private async Task DispatchAsync(Guid companyId, string recipient, Guid entityId, string entityType, string emailType, string subject, string body)
    {
        await _emailQueue.EnqueueAsync(new QueuedEmail(companyId, recipient, subject, body, emailType, entityType, entityId));
        await _emailSender.SendAsync(recipient, subject, body);
    }
}
