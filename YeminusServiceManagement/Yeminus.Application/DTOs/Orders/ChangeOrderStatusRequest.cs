using Yeminus.Domain.Enums;

namespace Yeminus.Application.DTOs.Orders;

public class ChangeOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
