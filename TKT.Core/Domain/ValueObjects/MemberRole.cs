using TKT.Core.Domain;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.ValueObjects;

public sealed class MemberRole
{
    private static readonly HashSet<string> Invitable = new(StringComparer.OrdinalIgnoreCase)
    {
        CompanyRoles.Admin,
        CompanyRoles.Agent,
        CompanyRoles.Member,
    };

    public string Value { get; }

    private MemberRole(string value) => Value = value;

    public static MemberRole CreateInvitable(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !Invitable.Contains(raw.Trim()))
            throw new ValidationException(InvitationErrors.RoleInvalid);

        return new MemberRole(raw.Trim().ToLowerInvariant());
    }
}
