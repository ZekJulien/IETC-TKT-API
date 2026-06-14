using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Identity.GetMe;

public sealed class GetMeUseCase(IAccountGateway accountGateway, IMembershipGateway membershipGateway)
    : IGetMeUseCase
{
    private readonly IAccountGateway _accountGateway = accountGateway;
    private readonly IMembershipGateway _membershipGateway = membershipGateway;

    public async Task<GetMeResult> ExecuteAsync(GetMeInput input)
    {
        var account = await _accountGateway.GetByIdAsync(input.AccountId)
            ?? throw new NotFoundException(AccountErrors.NotFound);

        var memberships = await _membershipGateway.GetActiveForAccountAsync(input.AccountId);
        var onboardingRequired = account.EmailConfirmed && memberships.Count == 0;

        return new GetMeResult(account.Email, account.EmailConfirmed, onboardingRequired, memberships);
    }
}
