namespace TKT.Core.Abstractions;

public interface IRefreshTokenIssuer
{
    Task<string> IssueAsync(Guid accountId, Guid? companyId = null);
}
