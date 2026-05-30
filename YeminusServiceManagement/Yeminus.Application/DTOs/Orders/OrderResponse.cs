using Yeminus.Domain.Enums;

namespace Yeminus.Application.DTOs.Orders;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientDocument { get; set; } = string.Empty;
    public Guid TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string Description { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
