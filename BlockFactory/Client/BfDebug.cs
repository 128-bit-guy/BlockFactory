using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using BlockFactory.Base;
using BlockFactory.Client.Render;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
[SuppressMessage("Usage", "CA2211")]
public static class BfDebug
{
    public static ImGuiController Controller = null!;
    private static readonly List<float> FrameDeltas = new();
    private static float _fps;
    private static int _fpsUpdateTime;

    public static void Init()
    {
        Controller = new ImGuiController(BfRendering.Gl, BlockFactoryClient.Window, BlockFactoryClient.InputContext);
    }

    public static void UpdateAndRender(double deltaTime)
    {
        Controller.Update((float)deltaTime);
        if (MouseInputManager.MouseIsEnabled)
            ImGui.GetIO().ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
        else
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NoMouse;
        if (ImGui.Begin("Performance", ImGuiWindowFlags.NoResize))
        {
            if (_fpsUpdateTime == 0)
            {
                _fps = 1 / (float)deltaTime;
                _fpsUpdateTime = 100;
            }
            else
            {
                --_fpsUpdateTime;
            }

            ImGui.Text($"FPS: {_fps}");
            FrameDeltas.Add((float)deltaTime);
            if (FrameDeltas.Count > 60) FrameDeltas.RemoveAt(0);

            var values = FrameDeltas.ToArray();
            ImGui.PlotHistogram(string.Empty, ref values[0], values.Length, 0,
                string.Empty, 0, 1 / 60f, new Vector2(300, 100));
        }

        ImGui.End();

        if (ImGui.Begin("Chunk loading"))
        {
            ImGui.Text($"Chunk loading progress: {BlockFactoryClient.Player.ChunkLoader!.Progress}");
            ImGui.Text($"Rendered chunks: {BlockFactoryClient.WorldRenderer.RenderedChunks}");
            ImGui.Text($"Fading out chunks: {BlockFactoryClient.WorldRenderer.FadingOutChunks}");
        }

        ImGui.End();
        Controller.Render();
    }

    public static void Destroy()
    {
        Controller.Dispose();
    }
}