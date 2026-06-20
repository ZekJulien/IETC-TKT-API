namespace TKT.Api.Contracts.Auth;

public sealed record TokenPairResponse(string AccessToken, string RefreshToken);
