using Dapper;
using Microsoft.Extensions.Logging;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

namespace Yeminus.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        IDbConnectionFactory factory,
        IPasswordHasher hasher,
        ILogger logger)
    {
        using var conn = factory.CreateConnection();
        conn.Open();

        var adminExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM users WHERE username = 'admin'");

        if (adminExists > 0)
        {
            logger.LogInformation("Seed: users already exist, skipping.");
            return;
        }

        logger.LogInformation("Seed: creating default users...");

        using var tx = conn.BeginTransaction();

        // ── Admin ─────────────────────────────────────────────────
        var adminPersonId = Guid.NewGuid();
        var adminUserId   = Guid.NewGuid();

        await conn.ExecuteAsync("""
            INSERT INTO persons (id, full_name, document_number, phone, email, created_at, updated_at)
            VALUES (@Id, 'System Administrator', '00000001', '+1000000000', 'admin@yeminus.com', NOW(), NOW())
            """, new { Id = adminPersonId }, tx);

        await conn.ExecuteAsync("""
            INSERT INTO users (id, person_id, username, password_hash, is_active, created_at)
            VALUES (@Id, @PersonId, 'admin', @Hash, TRUE, NOW())
            """, new { Id = adminUserId, PersonId = adminPersonId, Hash = hasher.Hash("Admin123!") }, tx);

        await conn.ExecuteAsync(
            "INSERT INTO user_roles (user_id, role_id) VALUES (@UserId, 1)",
            new { UserId = adminUserId }, tx);

        // ── Operator ──────────────────────────────────────────────
        var opPersonId = Guid.NewGuid();
        var opUserId   = Guid.NewGuid();

        await conn.ExecuteAsync("""
            INSERT INTO persons (id, full_name, document_number, phone, email, created_at, updated_at)
            VALUES (@Id, 'Service Operator', '00000002', '+2000000000', 'operator@yeminus.com', NOW(), NOW())
            """, new { Id = opPersonId }, tx);

        await conn.ExecuteAsync("""
            INSERT INTO users (id, person_id, username, password_hash, is_active, created_at)
            VALUES (@Id, @PersonId, 'operator', @Hash, TRUE, NOW())
            """, new { Id = opUserId, PersonId = opPersonId, Hash = hasher.Hash("Operator123!") }, tx);

        await conn.ExecuteAsync(
            "INSERT INTO user_roles (user_id, role_id) VALUES (@UserId, 2)",
            new { UserId = opUserId }, tx);

        // ── Órdenes de muestra ────────────────────────────────────
        await conn.ExecuteAsync("""
            INSERT INTO service_orders (id, client_id, technician_id, status, description, created_by, created_at, updated_at) VALUES
            (gen_random_uuid(), 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 1, 'Electrical panel inspection and maintenance', @UserId, NOW(), NOW()),
            (gen_random_uuid(), 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 2, 'Fix leaking pipe in bathroom', @UserId, NOW(), NOW()),
            (gen_random_uuid(), 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'ffffffff-ffff-ffff-ffff-ffffffffffff', 3, 'AC unit cleaning and gas recharge', @UserId, NOW(), NOW())
            """, new { UserId = adminUserId }, tx);

        tx.Commit();

        logger.LogInformation(
            "Seed completed. Users created: admin/Admin123! (Admin), operator/Operator123! (Operator)");
    }
}
