using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;

namespace TKT.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly string _issuer;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        _issuer = jwt["Issuer"]!;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
    }

    public string GenerateEmailConfirmationToken(Account account)
        => CreateToken(TokenPurpose.EmailConfirmation, TimeSpan.FromHours(24),
        [
            new Claim(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
            new Claim(AppClaims.SecurityStamp, account.SecurityStamp ?? string.Empty),
        ]);

    private string CreateToken(TokenPurpose purpose, TimeSpan lifetime, IEnumerable<Claim> claims)
    {
        var now = DateTime.UtcNow;
        var allClaims = new List<Claim>(claims) { new(AppClaims.Purpose, purpose.ToString()) };
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: $"{_issuer}.{purpose}",
            claims: allClaims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
