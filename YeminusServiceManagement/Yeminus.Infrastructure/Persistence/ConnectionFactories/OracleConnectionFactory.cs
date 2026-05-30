using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Yeminus.Infrastructure.Persistence.ConnectionFactories;

public class OracleConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new OracleConnection(connectionString);
}
