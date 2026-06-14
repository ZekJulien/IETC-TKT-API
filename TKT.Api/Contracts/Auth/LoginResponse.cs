namespace TKT.Api.Contracts.Auth;

public sealed record LoginResponse(string AccessToken, string RefreshToken);
