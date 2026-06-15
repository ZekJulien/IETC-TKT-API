namespace TKT.Api.Contracts.Auth;

public sealed record SwitchTenantResponse(string AccessToken, string RefreshToken);
