using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Yeminus.Application.DTOs.Auth;
using Yeminus.Application.Services;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Entities;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepo.Object,
            _roleRepo.Object,
            _hasher.Object,
            _tokenService.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsLoginResponse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashed",
            IsActive = true,
            Person = new Person { FullName = "Admin User", Email = "admin@yeminus.com" }
        };

        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("password123", "hashed")).Returns(true);
        _roleRepo.Setup(r => r.GetByUserIdAsync(user.Id)).ReturnsAsync([new Role { Name = "Admin" }]);
        _tokenService.Setup(t => t.GenerateToken(user, It.IsAny<IEnumerable<string>>())).Returns("jwt-token");
        _tokenService.Setup(t => t.GetExpiration()).Returns(DateTime.UtcNow.AddHours(8));

        var result = await _sut.LoginAsync(new LoginRequest { Username = "admin", Password = "password123" });

        Assert.NotNull(result);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("admin", result.User.Username);
    }

    [Fact]
    public async Task Login_UserNotFound_ThrowsUnauthorizedException()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("unknown")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.LoginAsync(new LoginRequest { Username = "unknown", Password = "pwd" }));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashed",
            IsActive = true
        };

        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrongpassword", "hashed")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.LoginAsync(new LoginRequest { Username = "admin", Password = "wrongpassword" }));
    }

    [Fact]
    public async Task Login_InactiveUser_ThrowsUnauthorizedException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashed",
            IsActive = false
        };

        _userRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.LoginAsync(new LoginRequest { Username = "admin", Password = "password" }));
    }
}
