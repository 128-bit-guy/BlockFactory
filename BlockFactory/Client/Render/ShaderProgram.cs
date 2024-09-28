using System.Drawing;
using BlockFactory.Base;
using BlockFactory.Utils;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class ShaderProgram : IDisposable
{
    private readonly int _model, _view, _projection;
    protected readonly uint Program;

    public ShaderProgram(string vertText, string fragText)
    {
        var vs = BfRendering.Gl.CreateShader(ShaderType.VertexShader);
        BfRendering.Gl.ShaderSource(vs, vertText);
        BfRendering.Gl.CompileShader(vs);
        var fs = BfRendering.Gl.CreateShader(ShaderType.FragmentShader);
        BfRendering.Gl.ShaderSource(fs, fragText);
        BfRendering.Gl.CompileShader(fs);
        Program = BfRendering.Gl.CreateProgram();
        BfRendering.Gl.AttachShader(Program, vs);
        BfRendering.Gl.AttachShader(Program, fs);
        BfRendering.Gl.LinkProgram(Program);
        Console.WriteLine(BfRendering.Gl.GetProgramInfoLog(Program));
        BfRendering.Gl.DeleteShader(vs);
        BfRendering.Gl.DeleteShader(fs);
        _model = GetUniformLocation("model");
        _view = GetUniformLocation("view");
        _projection = GetUniformLocation("projection");
    }

    public void Dispose()
    {
        BfRendering.Gl.DeleteProgram(Program);
    }

    public void Use()
    {
        BfRendering.Gl.UseProgram(Program);
    }

    protected int GetUniformLocation(string name)
    {
        return BfRendering.Gl.GetUniformLocation(Program, name);
    }

    protected void ShaderStorageBlockBinding(uint block, uint binding)
    {
        BfRendering.Gl.ShaderStorageBlockBinding(Program, block, binding);
    }
 
    protected void SetMatrix4(int uniform, Matrix4X4<float> mat)
    {
        BfRendering.Gl.ProgramUniformMatrix4(Program, uniform, 1, false, mat.Row1.X);
    }

    protected void SetVector3(int uniform, Vector3D<float> vec)
    {
        var vecs = vec.ToSystem();
        BfRendering.Gl.ProgramUniform3(Program, uniform, ref vecs);
    }

    protected unsafe void SetVector4(int uniform, Vector4D<float> vec)
    {
        Span<float> s = stackalloc float[4];
        for (var i = 0; i < 4; ++i) s[i] = vec[i];
        BfRendering.Gl.ProgramUniform4(Program, uniform, s);
    }

    protected void SetColor(int uniform, Color color)
    {
        SetVector4(uniform, color.AsVector());
    }

    protected void SetFloat(int uniform, float f)
    {
        BfRendering.Gl.ProgramUniform1(Program, uniform, f);
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