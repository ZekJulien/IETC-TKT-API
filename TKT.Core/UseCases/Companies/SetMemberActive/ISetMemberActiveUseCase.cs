namespace TKT.Core.UseCases.Companies.SetMemberActive;

public interface ISetMemberActiveUseCase
{
    Task<SetMemberActiveResult> ExecuteAsync(SetMemberActiveInput input);
}
