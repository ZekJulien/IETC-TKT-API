using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _key;
    private readonly TimeSpan _accessTokenLifetime;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        _issuer = jwt["Issuer"]!;
        _audience = jwt["Audience"]!;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        _accessTokenLifetime = TimeSpan.FromMinutes(double.Parse(jwt["ExpiresInMinutes"] ?? "60"));
    }

    public string GenerateEmailConfirmationToken(Account account)
        => CreateToken(EmailConfirmationAudience, TimeSpan.FromHours(24),
        [
            new Claim(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
            new Claim(AppClaims.SecurityStamp, account.SecurityStamp ?? string.Empty),
            new Claim(AppClaims.Purpose, TokenPurpose.EmailConfirmation.ToString()),
        ]);

    public EmailConfirmationToken ValidateEmailConfirmationToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = EmailConfirmationAudience,
            IssuerSigningKey = _key,
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(token, parameters, out _);

            if (principal.FindFirst(AppClaims.Purpose)?.Value != TokenPurpose.EmailConfirmation.ToString())
                throw new ValidationException(AuthErrors.ConfirmationInvalid);

            var accountId = Guid.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
            var securityStamp = principal.FindFirst(AppClaims.SecurityStamp)?.Value ?? string.Empty;
            return new EmailConfirmationToken(accountId, securityStamp);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            throw new ValidationException(AuthErrors.ConfirmationInvalid);
        }
    }

    public string GenerateAccessToken(Guid accountId, string email)
        => CreateToken(_audience, _accessTokenLifetime,
        [
            new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(AppClaims.Purpose, TokenPurpose.Access.ToString()),
        ]);

    private string EmailConfirmationAudience => $"{_issuer}.{TokenPurpose.EmailConfirmation}";

    private string CreateToken(string audience, TimeSpan lifetime, IEnumerable<Claim> claims)
    {
        var now = DateTime.UtcNow;
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
