using TKT.Core.Domain.Entities;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class ProfileMapper
{
    public static UserProfileRow ToRow(this UserProfile profile)
        => new()
        {
            AccountId = profile.AccountId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
        };

    public static UserProfile ToDomain(this UserProfileRow row)
        => new()
        {
            AccountId = row.AccountId,
            FirstName = row.FirstName,
            LastName = row.LastName,
        };
}
