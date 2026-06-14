namespace TKT.Core.Domain.Errors;

public static class InvitationErrors
{
    public const string NotFound = "invitation.not_found";
    public const string EmailMismatch = "invitation.email_mismatch";
    public const string AlreadyMember = "invitation.already_member";
}
