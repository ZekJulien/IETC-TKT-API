using TKT.Core.Abstractions;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.Services;

public sealed class AccessTokenIssuer(IMembershipGateway membershipGateway, ITokenService tokenService)
    : IAccessTokenIssuer
{
    private readonly IMembershipGateway _membershipGateway = membershipGateway;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<string> IssueForAsync(Guid accountId, string email)
    {
        var memberships = await _membershipGateway.GetActiveForAccountAsync(accountId);
        var activeTenant = memberships.Count == 1 ? memberships[0] : null;
        return _tokenService.GenerateAccessToken(accountId, email, activeTenant?.CompanyId, activeTenant?.Role);
    }

    public async Task<string> IssueForCompanyAsync(Guid accountId, string email, Guid companyId)
    {
        var memberships = await _membershipGateway.GetActiveForAccountAsync(accountId);
        var tenant = memberships.FirstOrDefault(m => m.CompanyId == companyId)
            ?? throw new ForbiddenException(CompanyErrors.Forbidden);
        return _tokenService.GenerateAccessToken(accountId, email, tenant.CompanyId, tenant.Role);
    }
}
