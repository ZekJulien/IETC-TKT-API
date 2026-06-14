using System.Text.RegularExpressions;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed partial class CompanySlug
{
    public string Value { get; }

    private CompanySlug(string value) => Value = value;

    public static CompanySlug Create(string? raw)
    {
        var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
        if (value.Length == 0) throw new ValidationException(CompanyErrors.SlugRequired);
        if (value.Length > 100) throw new ValidationException(CompanyErrors.SlugTooLong);
        if (!SlugPattern().IsMatch(value)) throw new ValidationException(CompanyErrors.SlugInvalid);
        return new CompanySlug(value);
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*[a-z0-9]$")]
    private static partial Regex SlugPattern();
}
