using Dapper;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Interfaces.Repositories;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

namespace Yeminus.Infrastructure.Persistence.Repositories;

public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = """
            SELECT u.id, u.person_id, u.username, u.password_hash, u.is_active, u.last_login, u.created_at,
                   p.id, p.full_name, p.document_number, p.phone, p.email, p.created_at, p.updated_at
            FROM users u
            INNER JOIN persons p ON p.id = u.person_id
            WHERE u.username = @Username AND u.is_active = true
            """;

        using var conn = connectionFactory.CreateConnection();
        var result = await conn.QueryAsync<User, Person, User>(
            sql,
            (user, person) => { user.Person = person; return user; },
            new { Username = username },
            splitOn: "id");

        return result.FirstOrDefault();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT u.id, u.person_id, u.username, u.password_hash, u.is_active, u.last_login, u.created_at,
                   p.id, p.full_name, p.document_number, p.phone, p.email, p.created_at, p.updated_at
            FROM users u
            INNER JOIN persons p ON p.id = u.person_id
            WHERE u.id = @Id
            """;

        using var conn = connectionFactory.CreateConnection();
        var result = await conn.QueryAsync<User, Person, User>(
            sql,
            (user, person) => { user.Person = person; return user; },
            new { Id = id },
            splitOn: "id");

        return result.FirstOrDefault();
    }

    public async Task UpdateLastLoginAsync(Guid userId, DateTime lastLogin)
    {
        const string sql = "UPDATE users SET last_login = @LastLogin WHERE id = @UserId";
        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(sql, new { UserId = userId, LastLogin = lastLogin });
    }
}
