using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Interfaces.Repositories;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;
using Yeminus.Infrastructure.Persistence.Repositories;
using Yeminus.Infrastructure.Security;

namespace Yeminus.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Permite que Dapper mapee columnas snake_case (is_active, password_hash, full_name…)
        // a propiedades PascalCase (IsActive, PasswordHash, FullName…)
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var factory = DbConnectionFactorySelector.Create(configuration);
        services.AddSingleton<IDbConnectionFactory>(_ => factory);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
