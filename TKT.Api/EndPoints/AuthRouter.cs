using TKT.Core.UseCases.Auth.Register;
using TKT.Api.Contracts.Auth;
using TKT.Api.Mappers;

namespace TKT.Api.EndPoints;

public static class AuthRouter
{
    public static WebApplication AddAuthRouter(this WebApplication app)
    {
        var group = app.MapGroup("api/auth").WithTags("auth");

        group.MapPost("register", async (RegisterRequest req, IRegisterAccountUseCase useCase) =>
        {
            await useCase.ExecuteAsync(req.ToInput());
            return Results.Created();
        });
        
        return app;
    }
}