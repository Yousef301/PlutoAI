using Pluto.DAL.Exceptions.Base;

namespace Pluto.DAL.Exceptions;

public class InvalidRequestException : BadRequestException
{
    public InvalidRequestException(string message) : base(message)
    {
    }

    public InvalidRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}