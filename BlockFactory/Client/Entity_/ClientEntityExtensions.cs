﻿using BlockFactory.Base;
using BlockFactory.Entity_;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Entity_;

[ExclusiveTo(Side.Client)]
public static class ClientEntityExtensions
{
    public static EntityPos GetInterpolatedPos(this Entity entity)
    {
        var cTime = GLFW.GetTime();
        var delta = cTime - entity.PrevTime;
        delta /= Constants.TickPeriod.TotalSeconds;
        delta = Math.Min(delta, 1.2d);
        var oPos = entity.Pos + entity.PrevPosDelta * (float)(1 - delta);
        return oPos;
    }

    public static void SetNewPos(this Entity entity, EntityPos newPos)
    {
        var cTime = GLFW.GetTime();
        var prevPos = entity.GetInterpolatedPos();
        var delta = prevPos - newPos;
        entity.Pos = newPos;
        entity.PrevPosDelta = delta.GetAbsolutePos();
        entity.PrevTime = cTime;
    }
}