using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TKT.Core.Abstractions;

namespace TKT.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly TimeSpan _slidingLifetime;
    private readonly TimeSpan _absoluteLifetime;

    public RefreshTokenService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        _slidingLifetime = TimeSpan.FromDays(double.Parse(jwt["RefreshTokenDays"] ?? "30"));
        _absoluteLifetime = TimeSpan.FromDays(double.Parse(jwt["RefreshTokenAbsoluteDays"] ?? "90"));
    }

    public GeneratedRefreshToken Generate(Guid accountId)
    {
        var secret = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var token = $"{accountId:N}.{secret}";
        var now = DateTimeOffset.UtcNow;
        return new GeneratedRefreshToken(token, Hash(token), now.Add(_slidingLifetime), now.Add(_absoluteLifetime));
    }

    public RefreshTokenParts? Parse(string token)
    {
        var separator = token.IndexOf('.');
        if (separator <= 0) return null;
        if (!Guid.TryParseExact(token[..separator], "N", out var accountId)) return null;
        return new RefreshTokenParts(accountId, Hash(token));
    }

    private static string Hash(string token)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
