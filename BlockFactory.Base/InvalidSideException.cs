using System.Runtime.Serialization;

namespace BlockFactory.Base;

public class InvalidSideException : Exception
{
    public InvalidSideException()
    {
    }

    protected InvalidSideException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidSideException(string? message) : base(message)
    {
    }

    public InvalidSideException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}