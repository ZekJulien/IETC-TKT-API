namespace TKT.Core.UseCases.Companies.RevokeInvitation;

public interface IRevokeInvitationUseCase
{
    Task ExecuteAsync(RevokeInvitationInput input);
}
