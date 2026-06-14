using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class Password
{
    public string Value { get; }
    private Password(string v) => Value = v;

    public static Password Create(string raw)
    {
        if (raw.Length < 8 || !raw.Any(char.IsUpper) || !raw.Any(char.IsDigit))
            throw new ValidationException(AuthErrors.PasswordWeak);
        return new Password(raw);
    }
}
