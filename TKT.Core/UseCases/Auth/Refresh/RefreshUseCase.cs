using TKT.Core.Abstractions;
using TKT.Core.Domain.Entities;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Auth.Refresh;

public sealed class RefreshUseCase(
    IRefreshTokenService refreshTokenService,
    IRefreshTokenGateway refreshTokenGateway,
    ISessionContextGateway sessionContextGateway,
    IAccountGateway accountGateway,
    IAccessTokenIssuer accessTokenIssuer) : IRefreshUseCase
{
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly IRefreshTokenGateway _refreshTokenGateway = refreshTokenGateway;
    private readonly ISessionContextGateway _sessionContextGateway = sessionContextGateway;
    private readonly IAccountGateway _accountGateway = accountGateway;
    private readonly IAccessTokenIssuer _accessTokenIssuer = accessTokenIssuer;

    public async Task<RefreshResult> ExecuteAsync(RefreshInput input)
    {
        var parts = _refreshTokenService.Parse(input.RefreshToken)
            ?? throw new InvalidCredentialsException();

        await _sessionContextGateway.SetCurrentUserAsync(parts.AccountId);

        var stored = await _refreshTokenGateway.GetByHashAsync(parts.Hash)
            ?? throw new InvalidCredentialsException();

        if (stored.IsRevoked || stored.ReplacedById is not null)
        {
            await _refreshTokenGateway.RevokeFamilyForReuseAsync(stored.FamilyId, stored.AccountId);
            throw new InvalidCredentialsException();
        }

        var now = DateTimeOffset.UtcNow;
        if (stored.ExpiresAt <= now || stored.AbsoluteExpiresAt <= now)
            throw new InvalidCredentialsException();

        var account = await _accountGateway.GetByIdAsync(stored.AccountId)
            ?? throw new InvalidCredentialsException();
        if (!account.IsActive) throw new ForbiddenException(AuthErrors.AccountDisabled);

        var generated = _refreshTokenService.Generate(stored.AccountId, parts.CompanyId);
        var rotatedExpiresAt = generated.ExpiresAt < stored.AbsoluteExpiresAt
            ? generated.ExpiresAt
            : stored.AbsoluteExpiresAt;

        var newTokenId = Guid.CreateVersion7();
        await _refreshTokenGateway.AddAsync(new RefreshToken
        {
            TokenId = newTokenId,
            AccountId = stored.AccountId,
            FamilyId = stored.FamilyId,
            TokenHash = generated.TokenHash,
            ExpiresAt = rotatedExpiresAt,
            AbsoluteExpiresAt = stored.AbsoluteExpiresAt,
        });
        await _refreshTokenGateway.MarkRotatedAsync(stored.TokenId, newTokenId);

        var accessToken = parts.CompanyId is { } companyId
            ? await _accessTokenIssuer.IssueForCompanyAsync(account.AccountId, account.Email, companyId)
            : await _accessTokenIssuer.IssueForAsync(account.AccountId, account.Email);

        return new RefreshResult(accessToken, generated.Token);
    }
}
