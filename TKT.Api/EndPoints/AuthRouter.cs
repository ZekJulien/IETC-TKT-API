using System.Security.Claims;
using TKT.Core.UseCases.Auth.ConfirmEmail;
using TKT.Core.UseCases.Auth.Login;
using TKT.Core.UseCases.Auth.Refresh;
using TKT.Core.UseCases.Auth.Register;
using TKT.Core.UseCases.Auth.SwitchTenant;
using TKT.Api.Contracts.Auth;
using TKT.Api.Extensions;
using TKT.Api.Mappers;

namespace TKT.Api.EndPoints;

public static class AuthRouter
{
    public static WebApplication AddAuthRouter(this WebApplication app)
    {
        var group = app.MapGroup("api/auth").WithTags("auth").AllowAnonymous();

        group.MapPost("register", async (RegisterRequest req, IRegisterAccountUseCase useCase) =>
        {
            await useCase.ExecuteAsync(req.ToInput());
            return Results.Created();
        });

        group.MapGet("confirm-email", async (string token, IConfirmEmailUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(new ConfirmEmailInput(token));
            return Results.Ok(result.ToResponse());
        });

        group.MapPost("login", async (LoginRequest req, ILoginUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput());
            return Results.Ok(result.ToResponse());
        });

        group.MapPost("refresh", async (RefreshRequest req, IRefreshUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput());
            return Results.Ok(result.ToResponse());
        });

        app.MapPost("api/auth/switch-tenant", async (SwitchTenantRequest req, ClaimsPrincipal user, ISwitchTenantUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(user.GetAccountId(), user.GetEmail()));
            return Results.Ok(result.ToResponse());
        }).WithTags("auth");

        return app;
    }
}
