namespace TKT.Core.Abstractions;

public interface IRefreshTokenService
{
    GeneratedRefreshToken Generate();
}
