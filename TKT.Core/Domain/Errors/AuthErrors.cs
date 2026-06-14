namespace TKT.Core.Domain.Errors;

public static class AuthErrors
{
    public const string EmailInvalid = "auth.email.invalid";
    public const string EmailAlreadyUsed = "auth.email.already_used";
    public const string EmailAlreadyConfirmed = "auth.email.already_confirmed";
    public const string PasswordWeak = "auth.password.weak";
    public const string PasswordMismatch = "auth.password.mismatch";
    public const string ConfirmationInvalid = "auth.confirmation.invalid";
    public const string AccountDisabled = "auth.account.disabled";
    public const string InvalidCredentials = "auth.invalid_credentials";
}
