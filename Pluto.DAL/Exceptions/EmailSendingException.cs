using Pluto.DAL.Exceptions.Base;

namespace Pluto.DAL.Exceptions;

public class EmailSendingException : InternalServerErrorException
{
    public EmailSendingException(string message) : base(message)
    {
    }

    public EmailSendingException(string message, Exception inner) : base(message, inner)
    {
    }
}