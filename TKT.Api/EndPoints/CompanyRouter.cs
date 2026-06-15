using System.Security.Claims;
using TKT.Api.Contracts.Companies;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Companies.ChangeMemberRole;
using TKT.Core.UseCases.Companies.InviteMember;
using TKT.Core.UseCases.Companies.ListMembers;
using TKT.Core.UseCases.Companies.RevokeInvitation;
using TKT.Core.UseCases.Companies.SetMemberActive;

namespace TKT.Api.EndPoints;

public static class CompanyRouter
{
    public static WebApplication AddCompanyRouter(this WebApplication app)
    {
        var group = app.MapGroup("api/companies/{companyId:guid}/members").WithTags("companies");

        group.MapPost("invite", async (Guid companyId, InviteMemberRequest req, ClaimsPrincipal user, IInviteMemberUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(companyId, user.GetCompanyId(), user.GetAccountId()));
            return Results.Ok(result.ToResponse());
        });

        group.MapGet("", async (Guid companyId, ClaimsPrincipal user, IListMembersUseCase useCase,
            int? page, int? pageSize, string? role, bool? active) =>
        {
            var input = new ListMembersInput(companyId, user.GetCompanyId(), user.GetAccountId(),
                page ?? 1, pageSize ?? 20, role, active);
            var result = await useCase.ExecuteAsync(input);
            return Results.Ok(result.ToResponse());
        });

        group.MapPatch("{accountId:guid}", async (Guid companyId, Guid accountId, ChangeMemberRoleRequest req,
            ClaimsPrincipal user, IChangeMemberRoleUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(companyId, accountId, user.GetCompanyId(), user.GetAccountId()));
            return Results.Ok(result.ToResponse());
        });

        group.MapPatch("{accountId:guid}/status", async (Guid companyId, Guid accountId, SetMemberStatusRequest req,
            ClaimsPrincipal user, ISetMemberActiveUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(companyId, accountId, user.GetCompanyId(), user.GetAccountId()));
            return Results.Ok(result.ToResponse());
        });

        app.MapDelete("api/companies/{companyId:guid}/invitations/{invitationId:guid}",
            async (Guid companyId, Guid invitationId, ClaimsPrincipal user, IRevokeInvitationUseCase useCase) =>
        {
            await useCase.ExecuteAsync(new RevokeInvitationInput(companyId, user.GetCompanyId(), user.GetAccountId(), invitationId));
            return Results.NoContent();
        }).WithTags("companies");

        return app;
    }
}
