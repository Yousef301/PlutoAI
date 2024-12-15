namespace Pluto.DAL.Exceptions;

public class EmailNotConfirmedException : UnauthorizedAccessException
{
    public EmailNotConfirmedException(string message) : base(message)
    {
    }

    public EmailNotConfirmedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}