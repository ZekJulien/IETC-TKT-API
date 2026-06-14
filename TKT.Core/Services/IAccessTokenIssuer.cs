namespace TKT.Core.Services;

public interface IAccessTokenIssuer
{
    Task<string> IssueForAsync(Guid accountId, string email);
}
