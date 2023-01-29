using BlockFactory.Side_;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class GLException : Exception
{
    public GLException(string message) : base(message)
    {
    }
}