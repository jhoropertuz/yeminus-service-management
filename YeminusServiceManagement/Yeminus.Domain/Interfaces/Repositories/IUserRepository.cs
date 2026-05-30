using Yeminus.Domain.Entities;

namespace Yeminus.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(Guid id);
    Task UpdateLastLoginAsync(Guid userId, DateTime lastLogin);
}
