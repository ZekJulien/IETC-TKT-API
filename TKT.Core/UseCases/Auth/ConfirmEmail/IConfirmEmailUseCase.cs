namespace TKT.Core.UseCases.Auth.ConfirmEmail;

public interface IConfirmEmailUseCase
{
    Task<ConfirmEmailResult> ExecuteAsync(ConfirmEmailInput input);
}
