using System.Data;

namespace Yeminus.Infrastructure.Persistence.ConnectionFactories;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
