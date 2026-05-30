using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Yeminus.Application.DTOs.Clients;
using Yeminus.Application.Services;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Tests.Services;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepo = new();
    private readonly Mock<IValidator<CreateClientRequest>> _createValidator = new();
    private readonly Mock<IValidator<UpdateClientRequest>> _updateValidator = new();
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _sut = new ClientService(
            _clientRepo.Object,
            _createValidator.Object,
            _updateValidator.Object,
            NullLogger<ClientService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsClientResponse()
    {
        var request = new CreateClientRequest
        {
            FullName = "John Doe",
            DocumentNumber = "12345678",
            Phone = "+1234567890",
            Email = "john@example.com",
            Address = "123 Main St"
        };

        var expectedId = Guid.NewGuid();
        var createdClient = new Client
        {
            Id = expectedId,
            PersonId = Guid.NewGuid(),
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            Person = new Person
            {
                FullName = request.FullName,
                DocumentNumber = request.DocumentNumber,
                Phone = request.Phone,
                Email = request.Email
            }
        };

        _createValidator.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _clientRepo.Setup(r => r.DocumentExistsAsync(request.DocumentNumber, null)).ReturnsAsync(false);
        _clientRepo.Setup(r => r.CreateAsync(It.IsAny<Client>(), request.FullName,
            request.DocumentNumber, request.Phone, request.Email))
            .ReturnsAsync(expectedId);
        _clientRepo.Setup(r => r.GetByIdAsync(expectedId)).ReturnsAsync(createdClient);

        var result = await _sut.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal("John Doe", result.FullName);
    }

    [Fact]
    public async Task CreateAsync_DuplicateDocument_ThrowsDomainValidationException()
    {
        var request = new CreateClientRequest
        {
            FullName = "John Doe",
            DocumentNumber = "12345678",
            Phone = "+1234567890",
            Email = "john@example.com",
            Address = "123 Main St"
        };

        _createValidator.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _clientRepo.Setup(r => r.DocumentExistsAsync(request.DocumentNumber, null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<DomainValidationException>(
            () => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _clientRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _clientRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(id));
    }
}
