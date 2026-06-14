using TKT.Api.Contracts.Auth;
using TKT.Core.UseCases.Auth.ConfirmEmail;
using TKT.Core.UseCases.Auth.Login;
using TKT.Core.UseCases.Auth.Register;

namespace TKT.Api.Mappers;

public static class AuthMapper
{
    public static RegisterInput ToInput(this RegisterRequest request)
        => new(request.Email, request.Password, request.ConfirmPassword);

    public static LoginInput ToInput(this LoginRequest request)
        => new(request.Email, request.Password);

    public static ConfirmEmailResponse ToResponse(this ConfirmEmailResult result)
        => new(result.AccessToken);

    public static LoginResponse ToResponse(this LoginResult result)
        => new(result.AccessToken, result.RefreshToken);
}
