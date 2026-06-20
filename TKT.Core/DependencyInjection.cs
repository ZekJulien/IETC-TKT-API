using Microsoft.Extensions.DependencyInjection;
using TKT.Core.Abstractions;
using TKT.Core.UseCases.Identity.GetMe;
using TKT.Core.UseCases.Identity.ListMyCompanies;
using TKT.Core.UseCases.Auth.ConfirmEmail;
using TKT.Core.UseCases.Auth.Login;
using TKT.Core.UseCases.Auth.Refresh;
using TKT.Core.UseCases.Auth.Register;
using TKT.Core.UseCases.Auth.SwitchTenant;
using TKT.Core.Services;
using TKT.Core.UseCases.Onboarding.CreateCompany;
using TKT.Core.UseCases.Onboarding.JoinInvitation;
using TKT.Core.UseCases.Companies.InviteMember;
using TKT.Core.UseCases.Companies.ListMembers;
using TKT.Core.UseCases.Companies.MemberDirectory;
using TKT.Core.UseCases.Companies.ChangeMemberRole;
using TKT.Core.UseCases.Companies.RevokeInvitation;
using TKT.Core.UseCases.Companies.SetMemberActive;
using TKT.Core.UseCases.Tickets.CreateTicket;
using TKT.Core.UseCases.Tickets.ListTickets;
using TKT.Core.UseCases.Tickets.GetTicket;
using TKT.Core.UseCases.Tickets.GetTicketStats;
using TKT.Core.UseCases.Tickets.UpdateTicket;
using TKT.Core.UseCases.Comments.CreateComment;
using TKT.Core.UseCases.Comments.ListComments;
using TKT.Core.UseCases.Comments.UpdateComment;

namespace TKT.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<IRegisterAccountUseCase, RegisterAccountUseCase>();
        services.AddScoped<IConfirmEmailUseCase, ConfirmEmailUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRefreshUseCase, RefreshUseCase>();
        services.AddScoped<ISwitchTenantUseCase, SwitchTenantUseCase>();
        services.AddScoped<IAccessTokenIssuer, AccessTokenIssuer>();
        services.AddScoped<IRefreshTokenIssuer, RefreshTokenIssuer>();
        services.AddScoped<ICreateCompanyUseCase, CreateCompanyUseCase>();
        services.AddScoped<IJoinInvitationUseCase, JoinInvitationUseCase>();
        services.AddScoped<IInviteMemberUseCase, InviteMemberUseCase>();
        services.AddScoped<IListMembersUseCase, ListMembersUseCase>();
        services.AddScoped<IMemberDirectoryUseCase, MemberDirectoryUseCase>();
        services.AddScoped<IChangeMemberRoleUseCase, ChangeMemberRoleUseCase>();
        services.AddScoped<ISetMemberActiveUseCase, SetMemberActiveUseCase>();
        services.AddScoped<IRevokeInvitationUseCase, RevokeInvitationUseCase>();
        services.AddScoped<IGetMeUseCase, GetMeUseCase>();
        services.AddScoped<IListMyCompaniesUseCase, ListMyCompaniesUseCase>();
        services.AddScoped<ICreateTicketUseCase, CreateTicketUseCase>();
        services.AddScoped<IListTicketsUseCase, ListTicketsUseCase>();
        services.AddScoped<IGetTicketUseCase, GetTicketUseCase>();
        services.AddScoped<IGetTicketStatsUseCase, GetTicketStatsUseCase>();
        services.AddScoped<IUpdateTicketUseCase, UpdateTicketUseCase>();
        services.AddScoped<ICreateCommentUseCase, CreateCommentUseCase>();
        services.AddScoped<IListCommentsUseCase, ListCommentsUseCase>();
        services.AddScoped<IUpdateCommentUseCase, UpdateCommentUseCase>();

        return services;
    }
}
