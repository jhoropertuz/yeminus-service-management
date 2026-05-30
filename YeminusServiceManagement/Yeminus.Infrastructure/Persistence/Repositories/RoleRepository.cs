using Dapper;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Interfaces.Repositories;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

namespace Yeminus.Infrastructure.Persistence.Repositories;

public class RoleRepository(IDbConnectionFactory connectionFactory) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId)
    {
        const string sql = """
            SELECT r.id, r.name, r.description
            FROM roles r
            INNER JOIN user_roles ur ON ur.role_id = r.id
            WHERE ur.user_id = @UserId
            """;

        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Role>(sql, new { UserId = userId });
    }
}
