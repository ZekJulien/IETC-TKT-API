namespace TKT.Core.IGateways;

public interface ISessionContextGateway
{
    Task SetCurrentUserAsync(Guid accountId);
}
