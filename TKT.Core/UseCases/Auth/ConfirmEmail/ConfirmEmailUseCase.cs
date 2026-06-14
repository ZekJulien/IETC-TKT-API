using TKT.Core.Abstractions;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Auth.ConfirmEmail;

public sealed class ConfirmEmailUseCase(IAccountGateway accountGateway, ITokenService tokenService)
    : IConfirmEmailUseCase
{
    private readonly IAccountGateway _accountGateway = accountGateway;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<ConfirmEmailResult> ExecuteAsync(ConfirmEmailInput input)
    {
        var confirmation = _tokenService.ValidateEmailConfirmationToken(input.Token);

        var account = await _accountGateway.GetByIdAsync(confirmation.AccountId)
            ?? throw new ValidationException(AuthErrors.ConfirmationInvalid);

        if (!account.IsActive) throw new ValidationException(AuthErrors.AccountDisabled);
        if (account.SecurityStamp != confirmation.SecurityStamp)
            throw new ValidationException(AuthErrors.ConfirmationInvalid);

        if (account.EmailConfirmed) throw new ConflictException(AuthErrors.EmailAlreadyConfirmed);
        await _accountGateway.SetEmailConfirmedAsync(account.AccountId);

        var accessToken = _tokenService.GenerateAccessToken(account.AccountId, account.Email);
        return new ConfirmEmailResult(accessToken);
    }
}
