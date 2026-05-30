using FluentValidation;
using Microsoft.Extensions.Logging;
using Yeminus.Application.DTOs.Orders;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Enums;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Application.Services;

public class ServiceOrderService(
    IServiceOrderRepository orderRepository,
    IClientRepository clientRepository,
    ITechnicianRepository technicianRepository,
    IValidator<CreateOrderRequest> createValidator,
    IValidator<ChangeOrderStatusRequest> statusValidator,
    ILogger<ServiceOrderService> logger) : IServiceOrderService
{
    public async Task<IEnumerable<OrderResponse>> GetAllAsync(OrderFilter? filter = null)
    {
        ServiceOrderFilter? domainFilter = null;
        if (filter != null)
        {
            domainFilter = new ServiceOrderFilter
            {
                Status = filter.Status,
                TechnicianName = filter.TechnicianName,
                Specialty = filter.Specialty,
                ClientName = filter.ClientName,
                ClientDocument = filter.ClientDocument,
                DateFrom = filter.DateFrom,
                DateTo = filter.DateTo
            };
        }

        var orders = await orderRepository.GetAllAsync(domainFilter);
        return orders.Select(MapToResponse);
    }

    public async Task<OrderResponse> GetByIdAsync(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("ServiceOrder", id);
        return MapToResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, Guid createdBy)
    {
        var validation = await createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var clientExists = await clientRepository.GetByIdAsync(request.ClientId);
        if (clientExists == null)
            throw new NotFoundException("Client", request.ClientId);

        var technicianExists = await technicianRepository.ExistsAsync(request.TechnicianId);
        if (!technicianExists)
            throw new NotFoundException("Technician", request.TechnicianId);

        var order = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            TechnicianId = request.TechnicianId,
            Status = OrderStatus.Pending,
            Description = request.Description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var id = await orderRepository.CreateAsync(order);
        logger.LogInformation("Service order created with ID {OrderId} by user {UserId}", id, createdBy);

        return await GetByIdAsync(id);
    }

    public async Task<OrderResponse> UpdateAsync(Guid id, UpdateOrderRequest request, Guid updatedBy)
    {
        var existing = await orderRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("ServiceOrder", id);

        var clientExists = await clientRepository.GetByIdAsync(request.ClientId);
        if (clientExists == null)
            throw new NotFoundException("Client", request.ClientId);

        var technicianExists = await technicianRepository.ExistsAsync(request.TechnicianId);
        if (!technicianExists)
            throw new NotFoundException("Technician", request.TechnicianId);

        var oldStatus = existing.Status;

        existing.ClientId = request.ClientId;
        existing.TechnicianId = request.TechnicianId;
        existing.Description = request.Description;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(existing);

        if (oldStatus != request.Status)
        {
            await orderRepository.ChangeStatusAsync(id, request.Status, updatedBy);
            logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}", id, oldStatus, request.Status);
        }

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await orderRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("ServiceOrder", id);

        await orderRepository.DeleteAsync(existing.Id);
        logger.LogInformation("Service order {OrderId} deleted", id);
    }

    public async Task<OrderResponse> ChangeStatusAsync(Guid id, ChangeOrderStatusRequest request, Guid changedBy)
    {
        var validation = await statusValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new DomainValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var existing = await orderRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("ServiceOrder", id);

        if (existing.Status == request.Status)
            throw new DomainValidationException($"Order is already in status '{request.Status}'.");

        await orderRepository.ChangeStatusAsync(id, request.Status, changedBy);

        logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus} by {UserId}",
            id, existing.Status, request.Status, changedBy);

        return await GetByIdAsync(id);
    }

    private static OrderResponse MapToResponse(ServiceOrder order) => new()
    {
        Id = order.Id,
        ClientId = order.ClientId,
        ClientName = order.Client?.Person?.FullName ?? string.Empty,
        ClientDocument = order.Client?.Person?.DocumentNumber ?? string.Empty,
        TechnicianId = order.TechnicianId,
        TechnicianName = order.Technician?.Person?.FullName ?? string.Empty,
        Specialty = order.Technician?.Specialty ?? string.Empty,
        Status = order.Status,
        Description = order.Description,
        CreatedBy = order.CreatedBy,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt
    };
}
