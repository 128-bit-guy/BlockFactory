using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Shader;

public class ShaderProgram
    {
        public int Program;
        public int ModelUniform;
        public int ViewUniform;
        public int ProjectionUniform;
        public ShaderProgram(string vertexPath, string fragmentPath)
        {
            //Create program
            Program = GL.CreateProgram();

            //Vertex shader
            int vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, ResourceLoader.GetResourceContent(vertexPath));
            GL.CompileShader(vShader);
            GLPreconditions.CheckShaderCompileError(vShader);
            GL.AttachShader(Program, vShader);

            //Fragment shader
            int fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, ResourceLoader.GetResourceContent(fragmentPath));
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

        public ShaderProgram(string pathBegin) : this(pathBegin + "_vertex.glsl", pathBegin + "_fragment.glsl")
        {

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