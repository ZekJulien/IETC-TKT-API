using TKT.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TKT.Infrastructure.Abstractions;
using TKT.Infrastructure.Security;
using TKT.Infrastructure.Email;
using TKT.Infrastructure.Gateways;
using TKT.Infrastructure.Repositories;
using TKT.Infrastructure.Repositories.Provisioning;
using TKT.Infrastructure.Repositories.Abstractions;
using TKT.Core.Abstractions;
using TKT.Core.IGateways;

namespace TKT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        Dapper.SqlMapper.AddTypeHandler(new IPAddressTypeHandler());
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<ISystemDbConnectionFactory, SystemNpgsqlConnectionFactory>();

        services.AddScoped<DbSession>();
        services.AddScoped<IDbSession>(sp => sp.GetRequiredService<DbSession>());
        services.AddScoped<SystemDbSession>();
        services.AddScoped<ISystemDbSession>(sp => sp.GetRequiredService<SystemDbSession>());
        services.AddScoped<IRequestContext, RequestContext>();
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IEmailSender, ConsoleEmailSender>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IAccountGateway, AccountGateway>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountLockoutRepository, AccountLockoutRepository>();

        services.AddScoped<IRefreshTokenGateway, RefreshTokenGateway>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IMembershipGateway, MembershipGateway>();
        services.AddScoped<IMembershipReadRepository, MembershipReadRepository>();

        services.AddScoped<ISessionContextGateway, SessionContextGateway>();

        services.AddScoped<ICompanyProvisioningGateway, CompanyProvisioningGateway>();
        services.AddScoped<ICompanyProvisioningRepository, CompanyProvisioningRepository>();

        services.AddScoped<ICompanyMemberProvisioningGateway, CompanyMemberProvisioningGateway>();
        services.AddScoped<ICompanyMemberProvisioningRepository, CompanyMemberProvisioningRepository>();

        services.AddScoped<IInvitationGateway, InvitationGateway>();
        services.AddScoped<IInvitationLookupRepository, InvitationLookupRepository>();

        return services;
    }
}
