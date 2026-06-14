namespace TKT.Core.UseCases.Auth.Login;

public interface ILoginUseCase
{
    Task<LoginResult> ExecuteAsync(LoginInput input);
}
