using System.Runtime.Serialization;

namespace BlockFactory.Client.Render;

public class GlException : Exception
{
    public GlException()
    {
    }

    protected GlException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public GlException(string? message) : base(message)
    {
    }

    public GlException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}