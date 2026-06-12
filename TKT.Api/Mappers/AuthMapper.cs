using TKT.Api.Contracts.Auth;
using TKT.Core.UseCases.Auth.Register;

namespace TKT.Api.Mappers;

public static class AuthMapper
{
    public static RegisterInput ToInput(this RegisterRequest input)
    {
        return new RegisterInput
        {
            Email = input.Email,
            Password = input.Password,
            ConfirmPassword = input.ConfirmPassword
        };
    }
}