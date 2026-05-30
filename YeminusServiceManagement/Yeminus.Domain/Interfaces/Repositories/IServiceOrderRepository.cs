using Yeminus.Domain.Entities;

namespace Yeminus.Domain.Interfaces.Repositories;

public interface IServiceOrderRepository
{
    Task<IEnumerable<ServiceOrder>> GetAllAsync(ServiceOrderFilter? filter = null);
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(ServiceOrder order);
    Task UpdateAsync(ServiceOrder order);
    Task DeleteAsync(Guid id);
    Task ChangeStatusAsync(Guid orderId, Domain.Enums.OrderStatus newStatus, Guid changedBy);
}

public class ServiceOrderFilter
{
    public Domain.Enums.OrderStatus? Status { get; set; }
    public string? TechnicianName { get; set; }
    public string? Specialty { get; set; }
    public string? ClientName { get; set; }
    public string? ClientDocument { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
