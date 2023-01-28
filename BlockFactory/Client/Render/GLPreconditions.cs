using OpenTK.Graphics.OpenGL4;

namespace BlockFactory.Client.Render;

public static class GLPreconditions
{
    public static void CheckShaderCompileError(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int compilationSuccessful);
        if (compilationSuccessful == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            throw new GLException(string.Format("Unable to compile shader: {0}", infoLog));
        }
    }
    public static void CheckProgramLinkError(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkSuccessful);
        if (linkSuccessful == 0)
        {
            string infoLog = GL.GetProgramInfoLog(program);
            throw new GLException(string.Format("Unable to link program: {0}", infoLog));
        }
    }

    public static void CheckGLError()
    {
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            throw new GLException(string.Format("GL error: {0}", error));
        }
    }
}