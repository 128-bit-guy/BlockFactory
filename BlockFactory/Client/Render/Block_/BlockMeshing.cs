﻿using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Registry_;
using BlockFactory.Util.Math_;

namespace BlockFactory.Client.Render.Block_;

public class BlockMeshing
{
    public static AttachmentRegistry<Block, IBlockMesher> Meshers { get; private set; } = null!;

    public static void Init()
    {
        Meshers = new AttachmentRegistry<Block, IBlockMesher>(Blocks.Registry);
        Meshers.Register(Blocks.Air, new AirBlockMesher());
        SetTextureAll(Blocks.Dirt, "BlockFactory.Client.Assets.Textures.dirt.png");
        SetTextureAll(Blocks.Stone, "BlockFactory.Client.Assets.Textures.stone.png");
        Meshers.Register(Blocks.Grass, new GrassBlockMesher());
        SetTexturesDirectional(Blocks.Log, "BlockFactory.Client.Assets.Textures.", "log_top.png",
            "log_side.png", "log_top.png");
        SetTextureAllNS(Blocks.Leaves, "BlockFactory.Client.Assets.Textures.leaves.png");
    }

    public static void SetTextures(Block block, string prefix, params string[] textures)
    {
        Meshers.Register(block, new PreSetTextureBlockMesher(textures.Select(s => prefix + s).ToArray(),
            true));
    }

    public static void SetTextureAll(Block block, string texture)
    {
        var textures = new string[DirectionUtils.GetValues().Length];
        Array.Fill(textures, texture);
        SetTextures(block, "", textures);
    }

    public static void SetTextureAllNS(Block block, string texture)
    {
        var textures = new string[DirectionUtils.GetValues().Length];
        Array.Fill(textures, texture);
        Meshers.Register(block, new PreSetTextureBlockMesher(textures, false));
    }

    public static void SetTexturesDirectional(Block block, string prefix, string top, string side, string bottom)
    {
        var textures = new string[DirectionUtils.GetValues().Length];
        Array.Fill(textures, side);
        textures[(int)Direction.Up] = top;
        textures[(int)Direction.Down] = bottom;
        SetTextures(block, prefix, textures);
    }
}