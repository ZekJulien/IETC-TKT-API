using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using TKT.Core.Abstractions;

namespace TKT.Infrastructure.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private readonly byte[] _pepper;

    public Argon2PasswordHasher(IConfiguration configuration)
        => _pepper = Encoding.UTF8.GetBytes(configuration["Security:PasswordPepper"]!);

    public string Hash(string password) => Argon2.Hash(Peppered(password));

    public bool Verify(string password, string hash) => Argon2.Verify(hash, Peppered(password));

    private string Peppered(string password)
    {
        using var hmac = new HMACSHA256(_pepper);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}
