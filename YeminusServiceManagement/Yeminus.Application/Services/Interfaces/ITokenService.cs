using Yeminus.Domain.Entities;

namespace Yeminus.Application.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    DateTime GetExpiration();
}
