using FluentValidation;
using Microsoft.Extensions.Logging;
using Yeminus.Application.DTOs.Technicians;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Application.Services;

public class TechnicianService(
    ITechnicianRepository technicianRepository,
    IValidator<CreateTechnicianRequest> createValidator,
    IValidator<UpdateTechnicianRequest> updateValidator,
    ILogger<TechnicianService> logger) : ITechnicianService
{
    public async Task<IEnumerable<TechnicianResponse>> GetAllAsync()
    {
        var technicians = await technicianRepository.GetAllAsync();
        return technicians.Select(MapToResponse);
    }

    public async Task<TechnicianResponse> GetByIdAsync(Guid id)
    {
        var technician = await technicianRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Technician", id);
        return MapToResponse(technician);
    }

    public async Task<TechnicianResponse> CreateAsync(CreateTechnicianRequest request)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var technician = new Technician
        {
            Id = Guid.NewGuid(),
            PersonId = Guid.NewGuid(),
            Specialty = request.Specialty,
            CreatedAt = DateTime.UtcNow
        };

        var id = await technicianRepository.CreateAsync(
            technician,
            request.FullName,
            request.DocumentNumber,
            request.Phone,
            request.Email);

        logger.LogInformation("Technician created with ID {TechnicianId}", id);

        return await GetByIdAsync(id);
    }

    public async Task<TechnicianResponse> UpdateAsync(Guid id, UpdateTechnicianRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var existing = await technicianRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Technician", id);

        existing.Specialty = request.Specialty;

        await technicianRepository.UpdateAsync(
            existing,
            request.FullName,
            request.DocumentNumber,
            request.Phone,
            request.Email);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await technicianRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Technician", id);

        await technicianRepository.DeleteAsync(existing.Id);
        logger.LogInformation("Technician {TechnicianId} deleted", id);
    }

    private static TechnicianResponse MapToResponse(Technician technician) => new()
    {
        Id = technician.Id,
        FullName = technician.Person?.FullName ?? string.Empty,
        DocumentNumber = technician.Person?.DocumentNumber ?? string.Empty,
        Phone = technician.Person?.Phone ?? string.Empty,
        Email = technician.Person?.Email ?? string.Empty,
        Specialty = technician.Specialty,
        CreatedAt = technician.CreatedAt
    };
}
