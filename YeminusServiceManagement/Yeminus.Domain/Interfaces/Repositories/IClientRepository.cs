using Yeminus.Domain.Entities;

namespace Yeminus.Domain.Interfaces.Repositories;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(Guid id);
    Task<bool> DocumentExistsAsync(string documentNumber, Guid? excludeClientId = null);
    Task<Guid> CreateAsync(Client client, string fullName, string documentNumber, string phone, string email);
    Task UpdateAsync(Client client, string fullName, string documentNumber, string phone, string email);
    Task DeleteAsync(Guid id);
}
