namespace TKT.Core.Abstractions;

public interface IRefreshTokenService
{
    GeneratedRefreshToken Generate(Guid accountId, Guid? companyId = null);

    RefreshTokenParts? Parse(string token);
}
