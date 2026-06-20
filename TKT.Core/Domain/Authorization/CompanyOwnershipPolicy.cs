using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;

namespace TKT.Core.Domain.Authorization;

public static class CompanyOwnershipPolicy
{
    public static void EnsureNotLastOwner(string targetRole, int activeOwnerCount)
    {
        if (string.Equals(targetRole, CompanyRoles.Owner, StringComparison.OrdinalIgnoreCase) && activeOwnerCount <= 1)
            throw new ConflictException(CompanyErrors.LastOwner);
    }
}
