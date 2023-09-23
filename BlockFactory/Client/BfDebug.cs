using System.Numerics;
using BlockFactory.Client.Render;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace BlockFactory.Client;

public static class BfDebug
{
    public static ImGuiController Controller;
    private static readonly List<float> _frameDeltas = new();
    private static float _fps;
    private static int _fpsUpdateTime = 0;

    public static void OnWindowLoad()
    {
        Controller = new ImGuiController(BfRendering.Gl, BlockFactoryClient.Window,
            BlockFactoryClient.Window.CreateInput());
    }

    public static void UpdateAndRender(double deltaTime)
    {
        Controller.Update((float)deltaTime);
        if (ImGui.Begin("Performance"))
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
            _frameDeltas.Add((float)deltaTime);
            if (_frameDeltas.Count > 60)
            {
                _frameDeltas.RemoveAt(0);
            }

            var values = _frameDeltas.ToArray();
            ImGui.PlotHistogram(string.Empty, ref values[0], values.Length, 0,
                string.Empty, 0, 1 / 60f, new Vector2(300, 100));
        }

        ImGui.End();
        Controller.Render();
    }

    public static void OnWindowClose()
    {
        Controller.Dispose();
    }
}