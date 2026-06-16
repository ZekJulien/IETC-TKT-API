using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class TicketTitle
{
    public const int MinLength = 3;
    public const int MaxLength = 255;

    public string Value { get; }

    private TicketTitle(string value) => Value = value;

    public static TicketTitle Create(string? raw)
    {
        var value = raw?.Trim() ?? string.Empty;
        if (value.Length < MinLength) throw new ValidationException(TicketErrors.TitleInvalid);
        if (value.Length > MaxLength) throw new ValidationException(TicketErrors.TitleTooLong);
        return new TicketTitle(value);
    }
}
