using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class TicketSource
{
    public const string Default = "web";

    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "web", "email", "api",
    };

    public string Value { get; }

    private TicketSource(string value) => Value = value;

    public static TicketSource CreateOrDefault(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new TicketSource(Default);
        var trimmed = raw.Trim();
        if (!Allowed.Contains(trimmed)) throw new ValidationException(TicketErrors.SourceInvalid);
        return new TicketSource(trimmed.ToLowerInvariant());
    }
}
