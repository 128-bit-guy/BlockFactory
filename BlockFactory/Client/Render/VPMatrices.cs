using BlockFactory.Client.Render.Shader;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render;

public class VPMatrices
{
    public Matrix4 View, Projection;

    public void Set(ShaderProgram program)
    {
        program.SetView(View);
        program.SetProjection(Projection);
    }
}