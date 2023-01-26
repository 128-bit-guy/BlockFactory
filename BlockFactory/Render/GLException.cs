using BlockFactory.Side_;

namespace BlockFactory.Render;

[ExclusiveTo(Side.Client)]
public class GLException : Exception
{
    public GLException(string message) : base(message)
    {
    }
}