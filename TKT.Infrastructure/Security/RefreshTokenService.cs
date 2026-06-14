using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TKT.Core.Abstractions;

namespace TKT.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly TimeSpan _lifetime;

    public RefreshTokenService(IConfiguration configuration)
        => _lifetime = TimeSpan.FromDays(double.Parse(configuration.GetSection("Jwt")["RefreshTokenDays"] ?? "30"));

    public GeneratedRefreshToken Generate()
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        return new GeneratedRefreshToken(token, hash, DateTimeOffset.UtcNow.Add(_lifetime));
    }
}
