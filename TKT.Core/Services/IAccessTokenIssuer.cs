namespace TKT.Core.Services;

public interface IAccessTokenIssuer
{
    Task<string> IssueForAsync(Guid accountId, string email);

    Task<string> IssueForCompanyAsync(Guid accountId, string email, Guid companyId);
}
