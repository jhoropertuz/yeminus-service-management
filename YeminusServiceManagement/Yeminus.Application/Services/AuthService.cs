using Microsoft.Extensions.Logging;
using Yeminus.Application.DTOs.Auth;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Exceptions;
using Yeminus.Domain.Interfaces.Repositories;

namespace Yeminus.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username)
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is disabled.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for user {Username}", request.Username);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var roles = (await roleRepository.GetByUserIdAsync(user.Id))
            .Select(r => r.Name)
            .ToList();

        var token = tokenService.GenerateToken(user, roles);
        var expiresAt = tokenService.GetExpiration();

        await userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

        logger.LogInformation("User {Username} logged in successfully", request.Username);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.Person?.FullName ?? string.Empty,
                Email = user.Person?.Email ?? string.Empty,
                Roles = roles
            }
        };
    }

    public async Task<UserResponse> GetCurrentUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var roles = (await roleRepository.GetByUserIdAsync(user.Id))
            .Select(r => r.Name)
            .ToList();

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.Person?.FullName ?? string.Empty,
            Email = user.Person?.Email ?? string.Empty,
            Roles = roles
        };
    }
}
