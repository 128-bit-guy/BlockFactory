using BlockFactory.Client.Render.Shader;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class VPMatrices
{
    public Matrix4 View, Projection;

    public void Set(ShaderProgram program)
    {
        program.SetView(View);
        program.SetProjection(Projection);
    }
}