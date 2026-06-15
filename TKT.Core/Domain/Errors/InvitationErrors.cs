namespace TKT.Core.Domain.Errors;

public static class InvitationErrors
{
    public const string NotFound = "invitation.not_found";
    public const string EmailMismatch = "invitation.email_mismatch";
    public const string AlreadyMember = "invitation.already_member";
    public const string AlreadyInvited = "invitation.already_invited";
    public const string RoleInvalid = "invitation.role_invalid";
    public const string QuotaExceeded = "invitation.quota_exceeded";
    public const string Forbidden = "invitation.forbidden";
}
