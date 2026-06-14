using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Auth.Login;

public sealed class LoginUseCase(
    IAccountGateway accountGateway,
    IMembershipGateway membershipGateway,
    ISessionContextGateway sessionContextGateway,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IRefreshTokenGateway refreshTokenGateway) : ILoginUseCase
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IAccountGateway _accountGateway = accountGateway;
    private readonly IMembershipGateway _membershipGateway = membershipGateway;
    private readonly ISessionContextGateway _sessionContextGateway = sessionContextGateway;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IRefreshTokenGateway _refreshTokenGateway = refreshTokenGateway;

    public async Task<LoginResult> ExecuteAsync(LoginInput input)
    {
        var email = Email.Create(input.Email);

        var account = await _accountGateway.GetByNormalizedEmailAsync(email.Normalized);
        if (account is null) throw new InvalidCredentialsException();

        if (IsLockedOut(account)) throw new ForbiddenException(AuthErrors.AccountLocked);

        if (!_passwordHasher.Verify(input.Password, account.PasswordHash))
        {
            await RegisterFailedAttemptAsync(account);
            throw new InvalidCredentialsException();
        }

        if (!account.IsActive) throw new ForbiddenException(AuthErrors.AccountDisabled);
        if (!account.EmailConfirmed) throw new ForbiddenException(AuthErrors.EmailNotConfirmed);

        await _sessionContextGateway.SetCurrentUserAsync(account.AccountId);

        var memberships = await _membershipGateway.GetActiveForAccountAsync(account.AccountId);
        var activeTenant = memberships.Count == 1 ? memberships[0] : null;

        var accessToken = _tokenService.GenerateAccessToken(
            account.AccountId, account.Email, activeTenant?.CompanyId, activeTenant?.Role);

        var refresh = _refreshTokenService.Generate();
        await _refreshTokenGateway.AddAsync(new RefreshToken
        {
            TokenId = Guid.CreateVersion7(),
            AccountId = account.AccountId,
            TokenHash = refresh.TokenHash,
            ExpiresAt = refresh.ExpiresAt,
            AbsoluteExpiresAt = refresh.ExpiresAt,
        });

        await _accountGateway.ResetLockoutAsync(account.AccountId);

        return new LoginResult(accessToken, refresh.Token);
    }

    private static bool IsLockedOut(Account account)
        => account.LockoutEnd is { } end && end > DateTimeOffset.UtcNow;

    private Task RegisterFailedAttemptAsync(Account account)
    {
        var failedCount = account.AccessFailedCount + 1;
        DateTimeOffset? lockoutEnd = null;
        if (failedCount >= MaxFailedAttempts)
        {
            lockoutEnd = DateTimeOffset.UtcNow.Add(LockoutDuration);
            failedCount = 0;
        }
        return _accountGateway.RegisterFailedLoginAsync(account.AccountId, failedCount, lockoutEnd);
    }
}
