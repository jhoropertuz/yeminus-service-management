using Dapper;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Interfaces.Repositories;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

namespace Yeminus.Infrastructure.Persistence.Repositories;

public class ClientRepository(IDbConnectionFactory connectionFactory) : IClientRepository
{
    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        const string sql = """
            SELECT c.id, c.person_id, c.address, c.created_at,
                   p.id, p.full_name, p.document_number, p.phone, p.email, p.created_at, p.updated_at
            FROM clients c
            INNER JOIN persons p ON p.id = c.person_id
            ORDER BY p.full_name
            """;

        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Client, Person, Client>(
            sql,
            (client, person) => { client.Person = person; return client; },
            splitOn: "id");
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT c.id, c.person_id, c.address, c.created_at,
                   p.id, p.full_name, p.document_number, p.phone, p.email, p.created_at, p.updated_at
            FROM clients c
            INNER JOIN persons p ON p.id = c.person_id
            WHERE c.id = @Id
            """;

        using var conn = connectionFactory.CreateConnection();
        var result = await conn.QueryAsync<Client, Person, Client>(
            sql,
            (client, person) => { client.Person = person; return client; },
            new { Id = id },
            splitOn: "id");

        return result.FirstOrDefault();
    }

    public async Task<bool> DocumentExistsAsync(string documentNumber, Guid? excludeClientId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM persons p
            INNER JOIN clients c ON c.person_id = p.id
            WHERE p.document_number = @DocumentNumber
              AND (@ExcludeClientId IS NULL OR c.id != @ExcludeClientId)
            """;

        using var conn = connectionFactory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(sql, new
        {
            DocumentNumber = documentNumber,
            ExcludeClientId = excludeClientId
        });
        return count > 0;
    }

    public async Task<Guid> CreateAsync(Client client, string fullName, string documentNumber, string phone, string email)
    {
        const string insertPerson = """
            INSERT INTO persons (id, full_name, document_number, phone, email, created_at, updated_at)
            VALUES (@Id, @FullName, @DocumentNumber, @Phone, @Email, @CreatedAt, @UpdatedAt)
            """;

        const string insertClient = """
            INSERT INTO clients (id, person_id, address, created_at)
            VALUES (@Id, @PersonId, @Address, @CreatedAt)
            """;

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(insertPerson, new
        {
            Id = client.PersonId,
            FullName = fullName,
            DocumentNumber = documentNumber,
            Phone = phone,
            Email = email,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.CreatedAt
        }, tx);

        await conn.ExecuteAsync(insertClient, new
        {
            client.Id,
            client.PersonId,
            client.Address,
            client.CreatedAt
        }, tx);

        tx.Commit();
        return client.Id;
    }

    public async Task UpdateAsync(Client client, string fullName, string documentNumber, string phone, string email)
    {
        const string updatePerson = """
            UPDATE persons
            SET full_name = @FullName, document_number = @DocumentNumber,
                phone = @Phone, email = @Email, updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        const string updateClient = """
            UPDATE clients SET address = @Address WHERE id = @ClientId
            """;

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(updatePerson, new
        {
            Id = client.PersonId,
            FullName = fullName,
            DocumentNumber = documentNumber,
            Phone = phone,
            Email = email,
            UpdatedAt = DateTime.UtcNow
        }, tx);

        await conn.ExecuteAsync(updateClient, new
        {
            Address = client.Address,
            ClientId = client.Id
        }, tx);

        tx.Commit();
    }

    public async Task DeleteAsync(Guid id)
    {
        const string getPersonId = "SELECT person_id FROM clients WHERE id = @Id";
        const string deleteClient = "DELETE FROM clients WHERE id = @Id";
        const string deletePerson = "DELETE FROM persons WHERE id = @PersonId";

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var personId = await conn.ExecuteScalarAsync<Guid>(getPersonId, new { Id = id }, tx);
        await conn.ExecuteAsync(deleteClient, new { Id = id }, tx);
        await conn.ExecuteAsync(deletePerson, new { PersonId = personId }, tx);

        tx.Commit();
    }
}
