using Microsoft.Extensions.Configuration;

namespace Yeminus.Infrastructure.Persistence.ConnectionFactories;

public static class DbConnectionFactorySelector
{
    public static IDbConnectionFactory Create(IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "PostgreSQL";
        return provider.ToUpperInvariant() switch
        {
            "ORACLE" => new OracleConnectionFactory(
                configuration.GetConnectionString("OracleConnection")
                    ?? throw new InvalidOperationException("OracleConnection string not configured.")),
            _ => new PostgresConnectionFactory(
                configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection string not configured."))
        };
    }
}
