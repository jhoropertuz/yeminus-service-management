using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Yeminus.Application.DTOs.Orders;
using Yeminus.Application.Services;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Enums;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Tests.Services;

public class ServiceOrderServiceTests
{
    private readonly Mock<IServiceOrderRepository> _orderRepo = new();
    private readonly Mock<IClientRepository> _clientRepo = new();
    private readonly Mock<ITechnicianRepository> _techRepo = new();
    private readonly Mock<IValidator<CreateOrderRequest>> _createValidator = new();
    private readonly Mock<IValidator<ChangeOrderStatusRequest>> _statusValidator = new();
    private readonly ServiceOrderService _sut;

    public ServiceOrderServiceTests()
    {
        _sut = new ServiceOrderService(
            _orderRepo.Object,
            _clientRepo.Object,
            _techRepo.Object,
            _createValidator.Object,
            _statusValidator.Object,
            NullLogger<ServiceOrderService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsOrderResponse()
    {
        var clientId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var request = new CreateOrderRequest
        {
            ClientId = clientId,
            TechnicianId = technicianId,
            Description = "Fix electrical panel"
        };

        var createdOrder = new ServiceOrder
        {
            Id = orderId,
            ClientId = clientId,
            TechnicianId = technicianId,
            Status = OrderStatus.Pending,
            Description = request.Description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Client = new Client { Id = clientId, Person = new Person { FullName = "Client A", DocumentNumber = "111" } },
            Technician = new Technician { Id = technicianId, Specialty = "Electrical", Person = new Person { FullName = "Tech A" } }
        };

        _createValidator.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _clientRepo.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(new Client { Id = clientId });
        _techRepo.Setup(r => r.ExistsAsync(technicianId)).ReturnsAsync(true);
        _orderRepo.Setup(r => r.CreateAsync(It.IsAny<ServiceOrder>())).ReturnsAsync(orderId);
        _orderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(createdOrder);

        var result = await _sut.CreateAsync(request, createdBy);

        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ClientNotFound_ThrowsNotFoundException()
    {
        var request = new CreateOrderRequest
        {
            ClientId = Guid.NewGuid(),
            TechnicianId = Guid.NewGuid(),
            Description = "Some description"
        };

        _createValidator.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _clientRepo.Setup(r => r.GetByIdAsync(request.ClientId)).ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateAsync(request, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_TechnicianNotFound_ThrowsNotFoundException()
    {
        var request = new CreateOrderRequest
        {
            ClientId = Guid.NewGuid(),
            TechnicianId = Guid.NewGuid(),
            Description = "Some description"
        };

        _createValidator.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _clientRepo.Setup(r => r.GetByIdAsync(request.ClientId))
            .ReturnsAsync(new Client { Id = request.ClientId });
        _techRepo.Setup(r => r.ExistsAsync(request.TechnicianId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateAsync(request, Guid.NewGuid()));
    }

    [Fact]
    public async Task ChangeStatusAsync_OrderNotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var statusRequest = new ChangeOrderStatusRequest { Status = OrderStatus.InProgress };

        _statusValidator.Setup(v => v.ValidateAsync(statusRequest, default))
            .ReturnsAsync(new ValidationResult());
        _orderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((ServiceOrder?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.ChangeStatusAsync(orderId, statusRequest, Guid.NewGuid()));
    }

    [Fact]
    public async Task ChangeStatusAsync_SameStatus_ThrowsDomainValidationException()
    {
        var orderId = Guid.NewGuid();
        var statusRequest = new ChangeOrderStatusRequest { Status = OrderStatus.Pending };

        var existingOrder = new ServiceOrder
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            Client = new Client { Person = new Person() },
            Technician = new Technician { Person = new Person() }
        };

        _statusValidator.Setup(v => v.ValidateAsync(statusRequest, default))
            .ReturnsAsync(new ValidationResult());
        _orderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(existingOrder);

        await Assert.ThrowsAsync<DomainValidationException>(
            () => _sut.ChangeStatusAsync(orderId, statusRequest, Guid.NewGuid()));
    }
}
