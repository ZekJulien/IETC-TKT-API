namespace TKT.Core.Services;

public sealed record CallerMembership(Guid CompanyId, string? Role);

public interface ICompanyMemberAuthorizer
{
    Task<CallerMembership?> ResolveAsync(Guid? callerCompanyId, Guid callerAccountId);

    Task<string?> ResolveForCompanyAsync(Guid? callerCompanyId, Guid targetCompanyId, Guid callerAccountId);
}
