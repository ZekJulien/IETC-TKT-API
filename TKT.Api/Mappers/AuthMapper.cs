using TKT.Api.Contracts.Auth;
using TKT.Core.UseCases.Auth.ConfirmEmail;
using TKT.Core.UseCases.Auth.Register;

namespace TKT.Api.Mappers;

public static class AuthMapper
{
    public static RegisterInput ToInput(this RegisterRequest request)
        => new(request.Email, request.Password, request.ConfirmPassword);

    public static ConfirmEmailResponse ToResponse(this ConfirmEmailResult result)
        => new(result.AccessToken);
}
