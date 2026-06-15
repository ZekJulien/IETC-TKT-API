namespace TKT.Core.UseCases.Companies.InviteMember;

public interface IInviteMemberUseCase
{
    Task<InviteMemberResult> ExecuteAsync(InviteMemberInput input);
}
