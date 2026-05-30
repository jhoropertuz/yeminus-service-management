using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Yeminus.Application.DTOs.Technicians;
using Yeminus.Application.Services;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Tests.Services;

public class TechnicianServiceTests
{
    private readonly Mock<ITechnicianRepository> _techRepo = new();
    private readonly Mock<IValidator<CreateTechnicianRequest>> _createValidator = new();
    private readonly Mock<IValidator<UpdateTechnicianRequest>> _updateValidator = new();
    private readonly TechnicianService _sut;

    public TechnicianServiceTests()
    {
        _sut = new TechnicianService(
            _techRepo.Object,
            _createValidator.Object,
            _updateValidator.Object,
            NullLogger<TechnicianService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsTechnicianResponse()
    {
        var request = new CreateTechnicianRequest
        {
            FullName = "Jane Smith",
            DocumentNumber = "87654321",
            Phone = "+0987654321",
            Email = "jane@example.com",
            Specialty = "Electrical"
        };

        var expectedId = Guid.NewGuid();
        var created = new Technician
        {
            Id = expectedId,
            PersonId = Guid.NewGuid(),
            Specialty = request.Specialty,
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
        _techRepo.Setup(r => r.CreateAsync(It.IsAny<Technician>(), request.FullName,
            request.DocumentNumber, request.Phone, request.Email))
            .ReturnsAsync(expectedId);
        _techRepo.Setup(r => r.GetByIdAsync(expectedId)).ReturnsAsync(created);

        var result = await _sut.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal("Electrical", result.Specialty);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _techRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Technician?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsDomainValidationException()
    {
        var request = new CreateTechnicianRequest { FullName = "" };
        var failures = new List<ValidationFailure>
        {
            new("FullName", "Full name is required.")
        };

        _createValidator.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult(failures));

        await Assert.ThrowsAsync<DomainValidationException>(() => _sut.CreateAsync(request));
    }
}
