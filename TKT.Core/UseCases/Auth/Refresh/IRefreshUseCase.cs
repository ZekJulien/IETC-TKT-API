namespace TKT.Core.UseCases.Auth.Refresh;

public interface IRefreshUseCase
{
    Task<RefreshResult> ExecuteAsync(RefreshInput input);
}
