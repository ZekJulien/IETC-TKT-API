namespace TKT.Core.UseCases.Auth.Register;

public interface IRegisterAccountUseCase
{
    Task ExecuteAsync(RegisterInput account);
}