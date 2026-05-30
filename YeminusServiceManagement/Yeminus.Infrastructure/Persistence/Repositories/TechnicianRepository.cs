using Dapper;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Interfaces.Repositories;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

namespace Yeminus.Infrastructure.Persistence.Repositories;

public class TechnicianRepository(IDbConnectionFactory connectionFactory) : ITechnicianRepository
{
    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        const string sql = """
            SELECT t.id, t.person_id, t.specialty, t.created_at,
                   p.id, p.full_name, p.document_number, p.phone, p.email, p.created_at, p.updated_at
            FROM technicians t
            INNER JOIN persons p ON p.id = t.person_id
            ORDER BY p.full_name
            """;

        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Technician, Person, Technician>(
            sql,
            (technician, person) => { technician.Person = person; return technician; },
            splitOn: "id");
    }

    public async Task<Technician?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT t.id, t.person_id, t.specialty, t.created_at,
                   p.id, p.full_name, p.document_number, p.phone, p.email, p.created_at, p.updated_at
            FROM technicians t
            INNER JOIN persons p ON p.id = t.person_id
            WHERE t.id = @Id
            """;

        using var conn = connectionFactory.CreateConnection();
        var result = await conn.QueryAsync<Technician, Person, Technician>(
            sql,
            (technician, person) => { technician.Person = person; return technician; },
            new { Id = id },
            splitOn: "id");

        return result.FirstOrDefault();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string sql = "SELECT COUNT(1) FROM technicians WHERE id = @Id";
        using var conn = connectionFactory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<Guid> CreateAsync(Technician technician, string fullName, string documentNumber, string phone, string email)
    {
        const string insertPerson = """
            INSERT INTO persons (id, full_name, document_number, phone, email, created_at, updated_at)
            VALUES (@Id, @FullName, @DocumentNumber, @Phone, @Email, @CreatedAt, @UpdatedAt)
            """;

        const string insertTechnician = """
            INSERT INTO technicians (id, person_id, specialty, created_at)
            VALUES (@Id, @PersonId, @Specialty, @CreatedAt)
            """;

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(insertPerson, new
        {
            Id = technician.PersonId,
            FullName = fullName,
            DocumentNumber = documentNumber,
            Phone = phone,
            Email = email,
            CreatedAt = technician.CreatedAt,
            UpdatedAt = technician.CreatedAt
        }, tx);

        await conn.ExecuteAsync(insertTechnician, new
        {
            technician.Id,
            technician.PersonId,
            technician.Specialty,
            technician.CreatedAt
        }, tx);

        tx.Commit();
        return technician.Id;
    }

    public async Task UpdateAsync(Technician technician, string fullName, string documentNumber, string phone, string email)
    {
        const string updatePerson = """
            UPDATE persons
            SET full_name = @FullName, document_number = @DocumentNumber,
                phone = @Phone, email = @Email, updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        const string updateTechnician = """
            UPDATE technicians SET specialty = @Specialty WHERE id = @TechnicianId
            """;

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(updatePerson, new
        {
            Id = technician.PersonId,
            FullName = fullName,
            DocumentNumber = documentNumber,
            Phone = phone,
            Email = email,
            UpdatedAt = DateTime.UtcNow
        }, tx);

        await conn.ExecuteAsync(updateTechnician, new
        {
            Specialty = technician.Specialty,
            TechnicianId = technician.Id
        }, tx);

        tx.Commit();
    }

    public async Task DeleteAsync(Guid id)
    {
        const string getPersonId = "SELECT person_id FROM technicians WHERE id = @Id";
        const string deleteTechnician = "DELETE FROM technicians WHERE id = @Id";
        const string deletePerson = "DELETE FROM persons WHERE id = @PersonId";

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var personId = await conn.ExecuteScalarAsync<Guid>(getPersonId, new { Id = id }, tx);
        await conn.ExecuteAsync(deleteTechnician, new { Id = id }, tx);
        await conn.ExecuteAsync(deletePerson, new { PersonId = personId }, tx);

        tx.Commit();
    }
}
