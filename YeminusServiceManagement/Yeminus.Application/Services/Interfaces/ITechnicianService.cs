using Yeminus.Application.DTOs.Technicians;

namespace Yeminus.Application.Services.Interfaces;

public interface ITechnicianService
{
    Task<IEnumerable<TechnicianResponse>> GetAllAsync();
    Task<TechnicianResponse> GetByIdAsync(Guid id);
    Task<TechnicianResponse> CreateAsync(CreateTechnicianRequest request);
    Task<TechnicianResponse> UpdateAsync(Guid id, UpdateTechnicianRequest request);
    Task DeleteAsync(Guid id);
}
