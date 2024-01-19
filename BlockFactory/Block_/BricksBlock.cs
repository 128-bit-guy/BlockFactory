﻿using BlockFactory.CubeMath;
using BlockFactory.World_.Light;

namespace BlockFactory.Block_;

public class BricksBlock : Block
{
    public override int GetTexture(CubeFace face)
    {
        return 16;
    }

    public override byte GetEmittedLight()
    {
        return 15;
    }
}