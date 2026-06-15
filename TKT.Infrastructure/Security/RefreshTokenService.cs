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

    public GeneratedRefreshToken Generate(Guid accountId, Guid? companyId = null)
    {
        var secret = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var token = companyId is { } company
            ? $"{accountId:N}.{company:N}.{secret}"
            : $"{accountId:N}.{secret}";
        var now = DateTimeOffset.UtcNow;
        return new GeneratedRefreshToken(token, Hash(token), now.Add(_slidingLifetime), now.Add(_absoluteLifetime));
    }

    public RefreshTokenParts? Parse(string token)
    {
        var segments = token.Split('.');
        if (segments.Length is not (2 or 3)) return null;
        if (!Guid.TryParseExact(segments[0], "N", out var accountId)) return null;

        Guid? companyId = null;
        if (segments.Length == 3)
        {
            if (!Guid.TryParseExact(segments[1], "N", out var company)) return null;
            companyId = company;
        }

        return new RefreshTokenParts(accountId, Hash(token), companyId);
    }

    private static string Hash(string token)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
