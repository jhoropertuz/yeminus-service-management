using System.Data;
using Npgsql;

namespace Yeminus.Infrastructure.Persistence.ConnectionFactories;

public class PostgresConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
