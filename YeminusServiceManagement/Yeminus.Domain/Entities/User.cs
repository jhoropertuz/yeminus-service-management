namespace Yeminus.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public Person? Person { get; set; }
    public ICollection<Role> Roles { get; set; } = [];
}
