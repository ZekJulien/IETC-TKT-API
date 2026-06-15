namespace TKT.Core.IGateways;

public interface ICompanySubscriptionGateway
{
    Task<int> GetMaxUsersAsync(Guid companyId);
}
