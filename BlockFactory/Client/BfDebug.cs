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
    private static readonly List<float> TickTimes = new();

    public static void Init()
    {
        Controller = new ImGuiController(BfRendering.Gl, BlockFactoryClient.Window, BlockFactoryClient.InputContext);
    }

    public static void UpdateAndRender(double deltaTime)
    {
        Controller.Update((float)deltaTime);
        if (MouseInputManager.ImGuiShouldBeFocused())
            ImGui.GetIO().ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
        else
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NoMouse;
        if (ImGui.Begin("Client Performance", ImGuiWindowFlags.NoResize))
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

        if (TickTimes.Count > 0)
        {
            if (ImGui.Begin("Server Performance", ImGuiWindowFlags.NoResize))
            {
                var values = TickTimes.ToArray();
                ImGui.Text($"Mean tick time: {values.Average()}ms");
                ImGui.PlotHistogram(string.Empty, ref values[0], values.Length, 0,
                    string.Empty, 0, Constants.TickFrequencyMs, new Vector2(300, 100));
            }

            ImGui.End();
        }

        if (BlockFactoryClient.Player != null)
        {
            if (ImGui.Begin("Chunk loading"))
            {
                ImGui.Text($"Chunk loading progress: {BlockFactoryClient.Player.ChunkLoader!.Progress}");
                ImGui.Text($"Rendered chunks: {BlockFactoryClient.WorldRenderer!.RenderedChunks}");
                ImGui.Text($"Fading out chunks: {BlockFactoryClient.WorldRenderer.FadingOutChunks}");
            }

            ImGui.End();
        }

        if (ImGui.Begin("Debug functions"))
        {
            if (ImGui.Button("Garbage collection"))
            {
                GC.Collect();
            }
        }

        ImGui.End();

        Controller.Render();
    }

    public static void HandleTickTime(float time)
    {
        TickTimes.Add(time);
        if (TickTimes.Count > 60)
        {
            TickTimes.RemoveAt(0);
        }
    }

    public static void Destroy()
    {
        Controller.Dispose();
    }
}