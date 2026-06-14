namespace TKT.Core.UseCases.Identity.GetMe;

public interface IGetMeUseCase
{
    Task<GetMeResult> ExecuteAsync(GetMeInput input);
}
