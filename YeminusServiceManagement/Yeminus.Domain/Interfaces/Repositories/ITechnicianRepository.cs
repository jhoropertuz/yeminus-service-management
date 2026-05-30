using Yeminus.Domain.Entities;

namespace Yeminus.Domain.Interfaces.Repositories;

public interface ITechnicianRepository
{
    Task<IEnumerable<Technician>> GetAllAsync();
    Task<Technician?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Guid> CreateAsync(Technician technician, string fullName, string documentNumber, string phone, string email);
    Task UpdateAsync(Technician technician, string fullName, string documentNumber, string phone, string email);
    Task DeleteAsync(Guid id);
}
