namespace TKT.Core.Abstractions;

public interface IInvitationNotifier
{
    Task NotifyMemberAddedAsync(Guid companyId, string recipientEmail, Guid membershipId);
    Task NotifyInvitationAsync(Guid companyId, string recipientEmail, Guid invitationId, string invitationCode);
}
