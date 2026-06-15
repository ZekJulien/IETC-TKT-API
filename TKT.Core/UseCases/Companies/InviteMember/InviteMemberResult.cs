namespace TKT.Core.UseCases.Companies.InviteMember;

public enum InviteMode
{
    DirectMember,
    PendingInvitation,
}

public sealed record InviteMemberResult(InviteMode Mode, string Role);
