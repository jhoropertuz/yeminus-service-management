using Yeminus.Domain.Enums;

namespace Yeminus.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}
