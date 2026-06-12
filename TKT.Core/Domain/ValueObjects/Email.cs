using System.ComponentModel.DataAnnotations;

namespace TKT.Core.Domain.ValueObjects;

public sealed class Email
{
    public string Value { get; }
    public string Normalized { get; }

    private Email(string value, string normalized) 
    {
        Value = value;
        Normalized = normalized;
    }   

    public static Email Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !raw.Contains('@'))  
            throw new ValidationException("Email invalide.");
        var trimmed = raw.Trim();
        return new Email(trimmed, trimmed.ToUpperInvariant());
    }
}
