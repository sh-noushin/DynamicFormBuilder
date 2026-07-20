namespace FormBuilder.Models.Exceptions;

public class ApiKeyNotFoundException : EntityNotFoundException
{
    public ApiKeyNotFoundException(Guid id) : base($"API key with ID '{id}' was not found.")
    {
    }
}
