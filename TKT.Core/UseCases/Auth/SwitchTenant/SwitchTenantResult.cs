namespace TKT.Core.UseCases.Auth.SwitchTenant;

public sealed record SwitchTenantResult(string AccessToken, string RefreshToken);
