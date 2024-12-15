using Pluto.DAL.Exceptions.Base;

namespace Pluto.DAL.Exceptions;

public class InvalidConfigurationException : InternalServerErrorException
{
    public InvalidConfigurationException(string message) : base(message)
    {
    }

    public InvalidConfigurationException(string message, Exception inner) : base(message, inner)
    {
    }
}