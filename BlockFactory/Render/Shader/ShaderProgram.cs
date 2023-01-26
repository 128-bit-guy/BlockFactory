using BlockFactory.Resource;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Render.Shader;

[ExclusiveTo(Side.Client)]
public class ShaderProgram : IDisposable
{
    public int ModelUniform;
    public int Program;
    public int ProjectionUniform;
    public int ViewUniform;

    public ShaderProgram(string vertexPath, string fragmentPath, IResourceLoader loader)
    {
        //Create program
        Program = GL.CreateProgram();

        //Vertex shader
        var vShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vShader, loader.GetResourceContent(vertexPath));
        GL.CompileShader(vShader);
        GLPreconditions.CheckShaderCompileError(vShader);
        GL.AttachShader(Program, vShader);

        //Fragment shader
        var fShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fShader, loader.GetResourceContent(fragmentPath));
        GL.CompileShader(fShader);
        GLPreconditions.CheckShaderCompileError(fShader);
        GL.AttachShader(Program, fShader);

        //Link program
        GL.LinkProgram(Program);
        GLPreconditions.CheckProgramLinkError(Program);

        //Delete shaders
        GL.DeleteShader(vShader);
        GL.DeleteShader(fShader);

        LoadUniforms();
    }

    public ShaderProgram(string pathBegin, IResourceLoader loader) : this(pathBegin + "_vertex.glsl",
        pathBegin + "_fragment.glsl", loader)
    {
    }

    public void Dispose()
    {
        GL.DeleteProgram(Program);
    }

    protected virtual void LoadUniforms()
    {
        //Program uniforms
        ModelUniform = GL.GetUniformLocation(Program, "model");
        ViewUniform = GL.GetUniformLocation(Program, "view");
        ProjectionUniform = GL.GetUniformLocation(Program, "projection");
    }

    public void Use()
    {
        GL.UseProgram(Program);
    }

    public void SetModel(Matrix4 model)
    {
        GL.ProgramUniformMatrix4(Program, ModelUniform, false, ref model);
        // GL.UniformMatrix4(ModelUniform, false, ref model);
    }

    public void SetView(Matrix4 view)
    {
        GL.ProgramUniformMatrix4(Program, ViewUniform, false, ref view);
        // GL.UniformMatrix4(ViewUniform, false, ref view);
    }

    public void SetProjection(Matrix4 projection)
    {
        GL.ProgramUniformMatrix4(Program, ProjectionUniform, false, ref projection);
        // GL.UniformMatrix4(ProjectionUniform, false, ref projection);
    }
}