using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Client.Render.Shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using BlockFactory.Item_;
using BlockFactory.Side_;

namespace BlockFactory.Client.Gui
{
    [ExclusiveTo(Side.Client)]
    public class Screen : IDisposable
    {
        public readonly BlockFactoryClient Client;
        public readonly MeshBuilder<GuiVertex> GuiMeshBuilder;
        public readonly RenderMesh<GuiVertex> GuiMesh;
        public readonly MeshBuilder<ColorVertex> ColorMeshBuilder;
        public readonly RenderMesh<ColorVertex> ColorMesh;
        private Vector2i _lastSize;
        public readonly List<Widget> Widgets;
        public Widget? ActiveWidget;

        public Screen(BlockFactoryClient client)
        {
            Client = client;
            GuiMeshBuilder = new MeshBuilder<GuiVertex>(VertexFormats.Gui);
            GuiMesh = new RenderMesh<GuiVertex>(VertexFormats.Gui);
            ColorMeshBuilder = new MeshBuilder<ColorVertex>(VertexFormats.Color);
            ColorMesh = new RenderMesh<ColorVertex>(VertexFormats.Color);
            _lastSize = new Vector2i(-1, -1);
            Widgets = new List<Widget>();
        }

        public virtual void OnShow() { 
            
        }

        public virtual void OnHide() {
            DestroyWidgets();
            _lastSize = new Vector2i(-1, -1);
        }

        public virtual void Dispose()
        {
            DestroyWidgets();
            GuiMesh.DeleteGl();
            ColorMesh.DeleteGl();
            GC.SuppressFinalize(this);
        }

        public virtual void UpdateAndRender() {
            DrawBackground();
            DrawWindow();
            Vector2i size = Client.GetDimensions();
            if (size != _lastSize) {
                _lastSize = size;
                DestroyWidgets();
                InitWidgets(size);
            }
            foreach (Widget widget in Widgets) { 
                widget.UpdateAndRender();
            }
        }

        public virtual void DrawWindow()
        {
            
        }

        public virtual void InitWidgets(Vector2i size) { 
            
        }

        protected void DestroyWidgets() {
            ActiveWidget = null;
            foreach (Widget widget in Widgets) {
                widget.Dispose();
            }
            Widgets.Clear();
        }

        public void DrawBackground() {
            var (width, height) = Client.GetDimensions();
            if (Client.GameInstance == null)
            {
                DrawTexturedRect(new(0, 0, width, height), 0, 64, Textures.DirtTexture);
            }
            else {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                DrawColoredRect(new(0, 0, width, height), 0, (0, 0, 0, 0.5f));
                GL.Disable(EnableCap.Blend);
            }
        }

        public void DrawTexturedRect(Box2 box, float zIndex, float textureScale, Texture2D texture)
        {
            GuiMeshBuilder.MatrixStack.Push();
            GuiMeshBuilder.BeginIndexSpace();
            GuiMeshBuilder.AddIndices(0, 2, 1, 0, 3, 2);
            float miX = box.Min.X;
            float maX = box.Max.X;
            float miY = box.Min.Y;
            float maY = box.Max.Y;
            float miTX = box.Min.X / textureScale;
            float maTX = box.Max.X / textureScale;
            float miTY = box.Min.Y / textureScale;
            float maTY = box.Max.Y / textureScale;
            GuiMeshBuilder.AddVertex((miX, miY, zIndex, 1, 1, 1, miTX, miTY));
            GuiMeshBuilder.AddVertex((maX, miY, zIndex, 1, 1, 1, maTX, miTY));
            GuiMeshBuilder.AddVertex((maX, maY, zIndex, 1, 1, 1, maTX, maTY));
            GuiMeshBuilder.AddVertex((miX, maY, zIndex, 1, 1, 1, miTX, maTY));
            GuiMeshBuilder.EndIndexSpace();
            GuiMeshBuilder.MatrixStack.Pop();
            GuiMeshBuilder.Upload(GuiMesh);
            GuiMeshBuilder.Reset();
            GuiMesh.Bind();
            Shaders.Gui.Use();
            Client.VpMatrices.Set(Shaders.Gui);
            Shaders.Gui.SetModel(Client.Matrices);
            texture.BindTexture();
            GL.DrawElements(PrimitiveType.Triangles, GuiMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawColoredRect(Box2 box, float zIndex, Vector4 color) {
            ColorMeshBuilder.MatrixStack.Push();
            ColorMeshBuilder.BeginIndexSpace();
            ColorMeshBuilder.AddIndices(0, 2, 1, 0, 3, 2);
            float miX = box.Min.X;
            float maX = box.Max.X;
            float miY = box.Min.Y;
            float maY = box.Max.Y;
            ColorMeshBuilder.AddVertex((miX, miY, zIndex, color.X, color.Y, color.Z, color.W));
            ColorMeshBuilder.AddVertex((maX, miY, zIndex, color.X, color.Y, color.Z, color.W));
            ColorMeshBuilder.AddVertex((maX, maY, zIndex, color.X, color.Y, color.Z, color.W));
            ColorMeshBuilder.AddVertex((miX, maY, zIndex, color.X, color.Y, color.Z, color.W));
            ColorMeshBuilder.EndIndexSpace();
            ColorMeshBuilder.MatrixStack.Pop();
            ColorMeshBuilder.Upload(ColorMesh);
            ColorMeshBuilder.Reset();
            ColorMesh.Bind();
            Shaders.Color.Use();
            Client.VpMatrices.Set(Shaders.Color);
            Shaders.Color.SetModel(Client.Matrices);
            Shaders.Color.SetColor(Vector4.One);
            GL.DrawElements(PrimitiveType.Triangles, ColorMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawText(ReadOnlySpan<char> text, int align) {
            GuiMeshBuilder.MatrixStack.Push();
            Textures.TextRenderer.Render(text, GuiMeshBuilder, align);
            GuiMeshBuilder.MatrixStack.Pop();
            GuiMeshBuilder.Upload(GuiMesh);
            GuiMeshBuilder.Reset();
            Shaders.Text.Use();
            Client.VpMatrices.Set(Shaders.Text);
            Shaders.Text.SetModel(Client.Matrices);
            Textures.TextRenderer.BindTexture();
            GuiMesh.Bind();
            GL.DrawElements(PrimitiveType.Triangles, GuiMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawStack(ItemStack stack)
        {
            Client.Matrices.Push();
            Client.Matrices.Scale(new Vector3(-32.0f, -32.0f, 1.0f));
            Client.Matrices.RotateX(-0.62f);
            Client.Matrices.RotateY(MathF.PI / 4);
            Client.Matrices.Translate((-0.5f, -0.5f, -0.5f));
            Client.ItemRenderer!.RenderItemStack(stack);
            Client.Matrices.Pop();
            if (!stack.IsEmpty())
            {
                Client.Matrices.Push();
                Client.Matrices.Translate(new Vector3(0f, 0f, 2f));
                Client.Matrices.Scale(0.5f);
                DrawText(stack.Count.ToString(), 0);
                Client.Matrices.Pop();
            }
        }
    }
}
