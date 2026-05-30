using Yeminus.Application.DTOs.Clients;

namespace Yeminus.Application.Services.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientResponse>> GetAllAsync();
    Task<ClientResponse> GetByIdAsync(Guid id);
    Task<ClientResponse> CreateAsync(CreateClientRequest request);
    Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request);
    Task DeleteAsync(Guid id);
}
