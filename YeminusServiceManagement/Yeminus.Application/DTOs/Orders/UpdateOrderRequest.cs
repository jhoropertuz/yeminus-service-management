using Yeminus.Domain.Enums;

namespace Yeminus.Application.DTOs.Orders;

public class UpdateOrderRequest
{
    public Guid ClientId { get; set; }
    public Guid TechnicianId { get; set; }
    public string Description { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
}
