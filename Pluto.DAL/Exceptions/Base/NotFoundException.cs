namespace Pluto.DAL.Exceptions.Base;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entity, int id) : base($"{entity} with ID {id} was not found.")
    {
    }

    public NotFoundException(string entity, string field, string name) : base(
        $"{entity} with {field} {name} was not found.")
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}