namespace TKT.Core.UseCases.Companies.ChangeMemberRole;

public interface IChangeMemberRoleUseCase
{
    Task<ChangeMemberRoleResult> ExecuteAsync(ChangeMemberRoleInput input);
}
