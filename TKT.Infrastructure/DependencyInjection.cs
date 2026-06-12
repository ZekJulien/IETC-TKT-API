using TKT.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TKT.Infrastructure.Abstractions;
using TKT.Infrastructure.Security;
using TKT.Infrastructure.Email;
using TKT.Infrastructure.Gateways;
using TKT.Infrastructure.Repositories;
using TKT.Infrastructure.Repositories.Abstractions;
using TKT.Core.Abstractions;
using TKT.Core.IGateways;

namespace TKT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        Dapper.SqlMapper.AddTypeHandler(new IPAddressTypeHandler());

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        services.AddScoped<DbSession>();
        services.AddScoped<IDbSession>(sp => sp.GetRequiredService<DbSession>());
        services.AddScoped<IRequestContext, RequestContext>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IEmailSender, ConsoleEmailSender>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<IAccountGateway, AccountGateway>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}
