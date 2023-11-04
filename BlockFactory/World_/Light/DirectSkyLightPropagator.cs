using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Light;

public static class DirectSkyLightPropagator
{
    [ThreadStatic] private static List<Vector3D<int>>[]? _updatePoses;

    private static void InitThreadStatics()
    {
        if(_updatePoses != null) return;
        _updatePoses = new List<Vector3D<int>>[Constants.ChunkSize + 1];
        for (var i = 0; i < Constants.ChunkSize + 1; ++i)
        {
            _updatePoses[i] = new List<Vector3D<int>>();
        }
    }

    private static int GetSupposedLight(ChunkNeighbourhood n, Vector3D<int> pos)
    {
        var oPos = pos + Vector3D<int>.UnitY;
        if (!n.GetBlockObj(pos).CanLightEnter(CubeFace.Top) ||
            !n.GetBlockObj(oPos).CanLightLeave(CubeFace.Bottom)) return 0;
        if (n.GetChunk(oPos.ShiftRight(Constants.ChunkSizeLog2))!.Data!.HasSkyLight)
        {
            return n.GetLight(oPos, LightChannel.DirectSky);
        }

        return n.GetBlock(oPos) == 0 ? 15 : 0;
    }
    public static void ProcessLightUpdates(Chunk chunk)
    {
        InitThreadStatics();
        chunk.Data!.HasSkyLight = true;
        foreach (var pos in chunk.ScheduledLightUpdates)
        {
            _updatePoses![(pos.Y & Constants.ChunkMask) + 1].Add(pos);
        }

        for (var i = Constants.ChunkSize; i >= 1; --i)
        {
            foreach (var pos in _updatePoses![i])
            {
                var l = GetSupposedLight(chunk.Neighbourhood, pos);
                if (l == chunk.GetLight(pos, LightChannel.DirectSky)) continue;
                chunk.SetLight(pos, LightChannel.DirectSky, (byte)l);
                chunk.ScheduleLightUpdate(pos);
                _updatePoses[pos.Y & Constants.ChunkMask].Add(pos - Vector3D<int>.UnitY);
            }
            _updatePoses[i].Clear();
        }

        foreach (var pos in _updatePoses![0])
        {
            chunk.Neighbourhood.ScheduleLightUpdate(pos);
        }
        _updatePoses[0].Clear();
    }
}