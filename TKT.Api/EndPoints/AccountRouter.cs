using System.Security.Claims;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Identity.GetMe;
using TKT.Core.UseCases.Identity.ListMyCompanies;

namespace TKT.Api.EndPoints;

public static class AccountRouter
{
    public static WebApplication AddAccountRouter(this WebApplication app)
    {
        app.MapGet("api/me", async (ClaimsPrincipal user, IGetMeUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(new GetMeInput(user.GetAccountId()));
            return Results.Ok(result.ToResponse());
        }).WithTags("account");

        app.MapGet("api/users/me/companies", async (ClaimsPrincipal user, IListMyCompaniesUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(new ListMyCompaniesInput(user.GetAccountId()));
            return Results.Ok(result.ToResponse());
        }).WithTags("account");

        return app;
    }
}
