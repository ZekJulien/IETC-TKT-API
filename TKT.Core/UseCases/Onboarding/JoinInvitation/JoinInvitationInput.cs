namespace TKT.Core.UseCases.Onboarding.JoinInvitation;

public sealed record JoinInvitationInput(Guid AccountId, string Email, string InvitationCode);
