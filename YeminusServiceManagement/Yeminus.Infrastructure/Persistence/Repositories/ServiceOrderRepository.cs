using System.Text;
using Dapper;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Enums;
using Yeminus.Domain.Interfaces.Repositories;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

namespace Yeminus.Infrastructure.Persistence.Repositories;

public class ServiceOrderRepository(IDbConnectionFactory connectionFactory) : IServiceOrderRepository
{
    public async Task<IEnumerable<ServiceOrder>> GetAllAsync(ServiceOrderFilter? filter = null)
    {
        var sql = new StringBuilder("""
            SELECT so.id, so.client_id, so.technician_id, so.status, so.description, so.created_by, so.created_at, so.updated_at,
                   c.id, c.person_id, c.address, c.created_at,
                   pc.id, pc.full_name, pc.document_number, pc.phone, pc.email, pc.created_at, pc.updated_at,
                   t.id, t.person_id, t.specialty, t.created_at,
                   pt.id, pt.full_name, pt.document_number, pt.phone, pt.email, pt.created_at, pt.updated_at
            FROM service_orders so
            INNER JOIN clients c ON c.id = so.client_id
            INNER JOIN persons pc ON pc.id = c.person_id
            INNER JOIN technicians t ON t.id = so.technician_id
            INNER JOIN persons pt ON pt.id = t.person_id
            WHERE 1=1
            """);

        var parameters = new DynamicParameters();

        if (filter != null)
        {
            if (filter.Status.HasValue)
            {
                sql.Append(" AND so.status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.TechnicianName))
            {
                sql.Append(" AND LOWER(pt.full_name) LIKE @TechnicianName");
                parameters.Add("TechnicianName", $"%{filter.TechnicianName.ToLower()}%");
            }
            if (!string.IsNullOrWhiteSpace(filter.Specialty))
            {
                sql.Append(" AND LOWER(t.specialty) LIKE @Specialty");
                parameters.Add("Specialty", $"%{filter.Specialty.ToLower()}%");
            }
            if (!string.IsNullOrWhiteSpace(filter.ClientName))
            {
                sql.Append(" AND LOWER(pc.full_name) LIKE @ClientName");
                parameters.Add("ClientName", $"%{filter.ClientName.ToLower()}%");
            }
            if (!string.IsNullOrWhiteSpace(filter.ClientDocument))
            {
                sql.Append(" AND pc.document_number LIKE @ClientDocument");
                parameters.Add("ClientDocument", $"%{filter.ClientDocument}%");
            }
            if (filter.DateFrom.HasValue)
            {
                sql.Append(" AND so.created_at >= @DateFrom");
                parameters.Add("DateFrom", filter.DateFrom.Value);
            }
            if (filter.DateTo.HasValue)
            {
                sql.Append(" AND so.created_at <= @DateTo");
                parameters.Add("DateTo", filter.DateTo.Value);
            }
        }

        sql.Append(" ORDER BY so.created_at DESC");

        using var conn = connectionFactory.CreateConnection();
        var orderDict = new Dictionary<Guid, ServiceOrder>();

        await conn.QueryAsync<ServiceOrder, Client, Person, Technician, Person, ServiceOrder>(
            sql.ToString(),
            (order, client, clientPerson, technician, techPerson) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var existing))
                {
                    existing = order;
                    client.Person = clientPerson;
                    technician.Person = techPerson;
                    existing.Client = client;
                    existing.Technician = technician;
                    orderDict.Add(existing.Id, existing);
                }
                return existing;
            },
            parameters,
            splitOn: "id,id,id,id");

        return orderDict.Values;
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT so.id, so.client_id, so.technician_id, so.status, so.description, so.created_by, so.created_at, so.updated_at,
                   c.id, c.person_id, c.address, c.created_at,
                   pc.id, pc.full_name, pc.document_number, pc.phone, pc.email, pc.created_at, pc.updated_at,
                   t.id, t.person_id, t.specialty, t.created_at,
                   pt.id, pt.full_name, pt.document_number, pt.phone, pt.email, pt.created_at, pt.updated_at
            FROM service_orders so
            INNER JOIN clients c ON c.id = so.client_id
            INNER JOIN persons pc ON pc.id = c.person_id
            INNER JOIN technicians t ON t.id = so.technician_id
            INNER JOIN persons pt ON pt.id = t.person_id
            WHERE so.id = @Id
            """;

        using var conn = connectionFactory.CreateConnection();
        ServiceOrder? result = null;

        await conn.QueryAsync<ServiceOrder, Client, Person, Technician, Person, ServiceOrder>(
            sql,
            (order, client, clientPerson, technician, techPerson) =>
            {
                client.Person = clientPerson;
                technician.Person = techPerson;
                order.Client = client;
                order.Technician = technician;
                result = order;
                return order;
            },
            new { Id = id },
            splitOn: "id,id,id,id");

        return result;
    }

    public async Task<Guid> CreateAsync(ServiceOrder order)
    {
        const string sql = """
            INSERT INTO service_orders (id, client_id, technician_id, status, description, created_by, created_at, updated_at)
            VALUES (@Id, @ClientId, @TechnicianId, @Status, @Description, @CreatedBy, @CreatedAt, @UpdatedAt)
            """;

        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            order.Id,
            order.ClientId,
            order.TechnicianId,
            Status = (int)order.Status,
            order.Description,
            order.CreatedBy,
            order.CreatedAt,
            order.UpdatedAt
        });

        return order.Id;
    }

    public async Task UpdateAsync(ServiceOrder order)
    {
        const string sql = """
            UPDATE service_orders
            SET client_id = @ClientId, technician_id = @TechnicianId,
                status = @Status, description = @Description, updated_at = @UpdatedAt
            WHERE id = @Id
            """;

        using var conn = connectionFactory.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            order.Id,
            order.ClientId,
            order.TechnicianId,
            Status = (int)order.Status,
            order.Description,
            order.UpdatedAt
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        const string deleteHistory = "DELETE FROM order_status_history WHERE service_order_id = @Id";
        const string deleteOrder = "DELETE FROM service_orders WHERE id = @Id";

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(deleteHistory, new { Id = id }, tx);
        await conn.ExecuteAsync(deleteOrder, new { Id = id }, tx);

        tx.Commit();
    }

    public async Task ChangeStatusAsync(Guid orderId, OrderStatus newStatus, Guid changedBy)
    {
        const string getOldStatus = "SELECT status FROM service_orders WHERE id = @Id";
        const string updateStatus = """
            UPDATE service_orders SET status = @Status, updated_at = @UpdatedAt WHERE id = @Id
            """;
        const string insertHistory = """
            INSERT INTO order_status_history (id, service_order_id, old_status, new_status, changed_by, changed_at)
            VALUES (@Id, @ServiceOrderId, @OldStatus, @NewStatus, @ChangedBy, @ChangedAt)
            """;

        using var conn = connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var oldStatus = await conn.ExecuteScalarAsync<int>(getOldStatus, new { Id = orderId }, tx);

        await conn.ExecuteAsync(updateStatus, new
        {
            Status = (int)newStatus,
            UpdatedAt = DateTime.UtcNow,
            Id = orderId
        }, tx);

        await conn.ExecuteAsync(insertHistory, new
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = (int)newStatus,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        }, tx);

        tx.Commit();
    }
}
