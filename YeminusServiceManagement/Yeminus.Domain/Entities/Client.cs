namespace Yeminus.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Person? Person { get; set; }
}
