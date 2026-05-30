using Yeminus.Application.DTOs.Orders;

namespace Yeminus.Application.Services.Interfaces;

public interface IServiceOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllAsync(OrderFilter? filter = null);
    Task<OrderResponse> GetByIdAsync(Guid id);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, Guid createdBy);
    Task<OrderResponse> UpdateAsync(Guid id, UpdateOrderRequest request, Guid updatedBy);
    Task DeleteAsync(Guid id);
    Task<OrderResponse> ChangeStatusAsync(Guid id, ChangeOrderStatusRequest request, Guid changedBy);
}
