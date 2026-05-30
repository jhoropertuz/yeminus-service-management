using FluentValidation;
using Microsoft.Extensions.Logging;
using Yeminus.Application.DTOs.Clients;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Application.Services;

public class ClientService(
    IClientRepository clientRepository,
    IValidator<CreateClientRequest> createValidator,
    IValidator<UpdateClientRequest> updateValidator,
    ILogger<ClientService> logger) : IClientService
{
    public async Task<IEnumerable<ClientResponse>> GetAllAsync()
    {
        var clients = await clientRepository.GetAllAsync();
        return clients.Select(MapToResponse);
    }

    public async Task<ClientResponse> GetByIdAsync(Guid id)
    {
        var client = await clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Client", id);
        return MapToResponse(client);
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        if (await clientRepository.DocumentExistsAsync(request.DocumentNumber))
            throw new DomainValidationException($"Document number '{request.DocumentNumber}' is already registered.");

        var client = new Client
        {
            Id = Guid.NewGuid(),
            PersonId = Guid.NewGuid(),
            Address = request.Address,
            CreatedAt = DateTime.UtcNow
        };

        var id = await clientRepository.CreateAsync(
            client,
            request.FullName,
            request.DocumentNumber,
            request.Phone,
            request.Email);

        logger.LogInformation("Client created with ID {ClientId}", id);

        return await GetByIdAsync(id);
    }

    public async Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var existing = await clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Client", id);

        if (await clientRepository.DocumentExistsAsync(request.DocumentNumber, id))
            throw new DomainValidationException($"Document number '{request.DocumentNumber}' is already registered.");

        existing.Address = request.Address;

        await clientRepository.UpdateAsync(
            existing,
            request.FullName,
            request.DocumentNumber,
            request.Phone,
            request.Email);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Client", id);

        await clientRepository.DeleteAsync(existing.Id);
        logger.LogInformation("Client {ClientId} deleted", id);
    }

    private static ClientResponse MapToResponse(Client client) => new()
    {
        Id = client.Id,
        FullName = client.Person?.FullName ?? string.Empty,
        DocumentNumber = client.Person?.DocumentNumber ?? string.Empty,
        Phone = client.Person?.Phone ?? string.Empty,
        Email = client.Person?.Email ?? string.Empty,
        Address = client.Address,
        CreatedAt = client.CreatedAt
    };
}
