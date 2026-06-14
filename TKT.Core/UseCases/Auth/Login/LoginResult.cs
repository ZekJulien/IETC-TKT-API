namespace TKT.Core.UseCases.Auth.Login;

public sealed record LoginResult(string AccessToken, string RefreshToken);
