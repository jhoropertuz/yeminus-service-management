using Yeminus.Domain.Enums;

namespace Yeminus.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid TechnicianId { get; set; }
    public OrderStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Client? Client { get; set; }
    public Technician? Technician { get; set; }
}
