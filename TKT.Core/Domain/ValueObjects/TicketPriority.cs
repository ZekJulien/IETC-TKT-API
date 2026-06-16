using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class TicketPriority
{
    public const string Default = "medium";

    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "low", "medium", "high", "urgent",
    };

    public string Value { get; }

    private TicketPriority(string value) => Value = value;

    public static TicketPriority CreateOrDefault(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new TicketPriority(Default);
        var trimmed = raw.Trim();
        if (!Allowed.Contains(trimmed)) throw new ValidationException(TicketErrors.PriorityInvalid);
        return new TicketPriority(trimmed.ToLowerInvariant());
    }
}
