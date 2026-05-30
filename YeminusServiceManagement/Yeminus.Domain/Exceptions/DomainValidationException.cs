namespace Yeminus.Domain.Exceptions;

public class DomainValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public DomainValidationException(string message) : base(message)
    {
        Errors = [message];
    }

    public DomainValidationException(IEnumerable<string> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
