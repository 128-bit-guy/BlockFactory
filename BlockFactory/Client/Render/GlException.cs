using System.Runtime.Serialization;
using BlockFactory.Base;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
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