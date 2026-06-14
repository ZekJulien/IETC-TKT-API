namespace TKT.Core.Abstractions;

public interface IRefreshTokenService
{
    GeneratedRefreshToken Generate(Guid accountId);

    RefreshTokenParts? Parse(string token);
}
