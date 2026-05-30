namespace Yeminus.Application.DTOs.Orders;

public class CreateOrderRequest
{
    public Guid ClientId { get; set; }
    public Guid TechnicianId { get; set; }
    public string Description { get; set; } = string.Empty;
}
