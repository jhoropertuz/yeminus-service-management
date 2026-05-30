using Yeminus.Domain.Enums;

namespace Yeminus.Application.DTOs.Orders;

public class OrderFilter
{
    public OrderStatus? Status { get; set; }
    public string? TechnicianName { get; set; }
    public string? Specialty { get; set; }
    public string? ClientName { get; set; }
    public string? ClientDocument { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
