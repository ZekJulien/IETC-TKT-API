using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class CommentText
{
    public const int MinLength = 1;
    public const int MaxLength = 10000;

    public string Value { get; }

    private CommentText(string value) => Value = value;

    public static CommentText Create(string? raw)
    {
        var value = raw?.Trim() ?? string.Empty;
        if (value.Length < MinLength) throw new ValidationException(CommentErrors.TextInvalid);
        if (value.Length > MaxLength) throw new ValidationException(CommentErrors.TextInvalid);
        return new CommentText(value);
    }
}
