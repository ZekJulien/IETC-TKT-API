using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Auth.Login;

public sealed class LoginUseCase(
    IAccountGateway accountGateway,
    ISessionContextGateway sessionContextGateway,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenIssuer refreshTokenIssuer) : ILoginUseCase
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IAccountGateway _accountGateway = accountGateway;
    private readonly ISessionContextGateway _sessionContextGateway = sessionContextGateway;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IAccessTokenIssuer _accessTokenIssuer = accessTokenIssuer;
    private readonly IRefreshTokenIssuer _refreshTokenIssuer = refreshTokenIssuer;

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

        var accessToken = await _accessTokenIssuer.IssueForAsync(account.AccountId, account.Email);
        var refreshToken = await _refreshTokenIssuer.IssueAsync(account.AccountId);

        await _accountGateway.ResetLockoutAsync(account.AccountId);

        return new LoginResult(accessToken, refreshToken);
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
