namespace TKT.Core.Abstractions;

public sealed record GeneratedRefreshToken(
    string Token,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset AbsoluteExpiresAt);
