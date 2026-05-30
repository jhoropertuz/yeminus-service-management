using Yeminus.Domain.Entities;

namespace Yeminus.Domain.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId);
}
