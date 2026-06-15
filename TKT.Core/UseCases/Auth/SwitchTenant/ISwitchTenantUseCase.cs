namespace TKT.Core.UseCases.Auth.SwitchTenant;

public interface ISwitchTenantUseCase
{
    Task<SwitchTenantResult> ExecuteAsync(SwitchTenantInput input);
}
