namespace Yeminus.Domain.Entities;

public class Technician
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Person? Person { get; set; }
}
