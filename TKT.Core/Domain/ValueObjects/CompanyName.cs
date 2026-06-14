using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class CompanyName
{
    public string Value { get; }

    private CompanyName(string value) => Value = value;

    public static CompanyName Create(string? raw)
    {
        var value = raw?.Trim() ?? string.Empty;
        if (value.Length == 0) throw new ValidationException(CompanyErrors.NameRequired);
        if (value.Length > 255) throw new ValidationException(CompanyErrors.NameTooLong);
        return new CompanyName(value);
    }
}
