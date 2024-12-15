namespace Pluto.DAL.Exceptions;

public class InvalidTokenException : UnauthorizedAccessException
{
    public InvalidTokenException(string message) : base(message)
    {
    }

    public InvalidTokenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}