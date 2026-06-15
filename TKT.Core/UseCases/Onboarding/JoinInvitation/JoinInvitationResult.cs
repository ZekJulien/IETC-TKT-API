namespace TKT.Core.UseCases.Onboarding.JoinInvitation;

public sealed record JoinInvitationResult(
    Guid CompanyId,
    string Role,
    string AccessToken,
    string RefreshToken);
