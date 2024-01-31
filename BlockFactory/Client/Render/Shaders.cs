﻿using BlockFactory.Base;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public static class Shaders
{
    public static BlockShaderProgram Block = null!;
    public static ShaderProgram Text = null!;
    public static ShaderProgram Gui = null!;

    public static void Init()
    {
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Block.Fragment.glsl")!;
            Block = new BlockShaderProgram(vertText, fragText);
        }
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Text.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Text.Fragment.glsl")!;
            Text = new ShaderProgram(vertText, fragText);
        }
        {
            var vertText = BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Gui.Vertex.glsl")!;
            var fragText =
                BlockFactoryClient.ResourceLoader.GetResourceText("BlockFactory.Shaders.Gui.Fragment.glsl")!;
            Gui = new ShaderProgram(vertText, fragText);
        }
    }

    public static void Destroy()
    {
        Block.Dispose();
    }
}