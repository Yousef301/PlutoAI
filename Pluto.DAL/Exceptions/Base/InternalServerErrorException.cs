namespace Pluto.DAL.Exceptions.Base;

public class InternalServerErrorException : Exception
{
    public InternalServerErrorException(string message) : base(message)
    {
    }

    public InternalServerErrorException(string message, Exception inner) : base(message, inner)
    {
    }
}