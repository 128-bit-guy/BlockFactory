using BlockFactory.Render.Shader;
using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Render;

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