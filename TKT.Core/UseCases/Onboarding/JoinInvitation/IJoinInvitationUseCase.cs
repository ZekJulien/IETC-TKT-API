namespace TKT.Core.UseCases.Onboarding.JoinInvitation;

public interface IJoinInvitationUseCase
{
    Task<JoinInvitationResult> ExecuteAsync(JoinInvitationInput input);
}
