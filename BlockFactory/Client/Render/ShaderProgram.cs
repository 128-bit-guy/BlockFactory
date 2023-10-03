using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class ShaderProgram : IDisposable
{
    private readonly int _model, _view, _projection;
    private readonly uint _program;

    public ShaderProgram(string vertText, string fragText)
    {
        var vs = BfRendering.Gl.CreateShader(ShaderType.VertexShader);
        BfRendering.Gl.ShaderSource(vs, vertText);
        BfRendering.Gl.CompileShader(vs);
        var fs = BfRendering.Gl.CreateShader(ShaderType.FragmentShader);
        BfRendering.Gl.ShaderSource(fs, fragText);
        BfRendering.Gl.CompileShader(fs);
        _program = BfRendering.Gl.CreateProgram();
        BfRendering.Gl.AttachShader(_program, vs);
        BfRendering.Gl.AttachShader(_program, fs);
        BfRendering.Gl.LinkProgram(_program);
        BfRendering.Gl.DeleteShader(vs);
        BfRendering.Gl.DeleteShader(fs);
        _model = GetUniformLocation("model");
        _view = GetUniformLocation("view");
        _projection = GetUniformLocation("projection");
    }

    public void Dispose()
    {
        BfRendering.Gl.DeleteProgram(_program);
    }

    public void Use()
    {
        BfRendering.Gl.UseProgram(_program);
    }

    protected int GetUniformLocation(string name)
    {
        return BfRendering.Gl.GetUniformLocation(_program, name);
    }

    protected void SetMatrix4(int uniform, Matrix4X4<float> mat)
    {
        BfRendering.Gl.ProgramUniformMatrix4(_program, uniform, 1, false, mat.Row1.X);
    }

    public void SetModel(Matrix4X4<float> model)
    {
        SetMatrix4(_model, model);
    }

    public void SetView(Matrix4X4<float> view)
    {
        SetMatrix4(_view, view);
    }

    public void SetProjection(Matrix4X4<float> projection)
    {
        SetMatrix4(_projection, projection);
    }
}