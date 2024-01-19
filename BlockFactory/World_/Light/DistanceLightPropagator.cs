using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Math_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Light;

public static class DistanceLightPropagator
{
    private static readonly LightChannel[] Channels = Enum.GetValues<LightChannel>();
    [ThreadStatic] private static Queue<(Vector3D<int> pos, bool changed)>? _removeLightQueue;

    [ThreadStatic] private static List<Vector3D<int>>? _beginAddLightList;

    [ThreadStatic] private static List<Vector3D<int>>[]? _addLightQueue;

    private static void InitThreadStatics()
    {
        _removeLightQueue ??= new Queue<(Vector3D<int> pos, bool changed)>();
        _beginAddLightList ??= new List<Vector3D<int>>();
        if (_addLightQueue == null)
        {
            _addLightQueue = new List<Vector3D<int>>[16];
            for (int i = 1; i < 16; ++i)
            {
                _addLightQueue[i] = new List<Vector3D<int>>();
            }
        }
    }

    private static int GetEmittedLight(IBlockAccess n, Vector3D<int> pos, LightChannel channel)
    {
        if (channel == LightChannel.Block)
        {
            return n.GetBlockObj(pos).GetEmittedLight();
        }
        else
        {
            return n.GetLight(pos, LightChannel.DirectSky);
        }
    }
    
    private static int GetSupposedLight(IBlockAccess n, Vector3D<int> pos, LightChannel channel)
    {
        int cLight = GetEmittedLight(n, pos, channel);
        foreach (var face in CubeFaceUtils.Values())
        {
            var oPos = pos + face.GetDelta();
            var lightFromNeighbor = n.GetLight(oPos, channel) - 1;
            if(lightFromNeighbor <= cLight) continue;
            var lightCanPass = n.GetBlockObj(pos).CanLightEnter(face, channel) &&
                               n.GetBlockObj(oPos).CanLightLeave(face.GetOpposite(), channel);
            if(!lightCanPass) continue;
            cLight = lightFromNeighbor;
        }

        return cLight;
    }

    public static void ProcessLightUpdates(Chunk c)
    {
        var n = c.Neighbourhood;
        InitThreadStatics();

        foreach (var channel in Channels)
        {
            if(channel == LightChannel.DirectSky) continue;
            foreach (var pos in c.ScheduledLightUpdates)
            {
                var sl = GetSupposedLight(n, pos, channel);
                var l = n.GetLight(pos, channel);
                if (sl > l)
                {
                    bool shouldBfs = false;
                    foreach (CubeFace face in CubeFaceUtils.Values())
                    {
                        var ol = GetEmittedLight(n, pos + face.GetDelta(), channel);
                        if (ol < sl - 1)
                        {
                            shouldBfs = true;
                            break;
                        }
                    }

                    if (shouldBfs)
                    {
                        _beginAddLightList!.Add(pos);
                    }
                    else
                    {
                        n.SetLight(pos, channel, (byte)sl);
                    }
                } else if (sl < l)
                {
                    _removeLightQueue!.Enqueue((pos, true));
                }
            }

            while (_removeLightQueue!.Count > 0)
            {
                var (pos, changed) = _removeLightQueue.Dequeue();
                var light = n.GetLight(pos, channel);
                if (light != 0 || changed)
                {
                    _beginAddLightList!.Add(pos);
                }

                if (light == 0) continue;
                n.SetLight(pos, channel, 0);

                foreach (var face in CubeFaceUtils.Values())
                {
                    var oPos = pos + face.GetDelta();
                    var lightCanPass = (changed || n.GetBlockObj(pos).CanLightLeave(face, channel)) &&
                                       n.GetBlockObj(oPos).CanLightEnter(face.GetOpposite(), channel);
                    if (lightCanPass && n.GetLight(oPos, channel) < light)
                    {
                        _removeLightQueue.Enqueue((oPos, false));
                    }
                }
            }

            foreach (var pos in _beginAddLightList!)
            {
                var cLight = GetSupposedLight(n, pos, channel);
                if (cLight > 0)
                {
                    _addLightQueue![cLight].Add(pos);
                }
            }
            
            _beginAddLightList.Clear();

            for (var cLight = 15; cLight > 0; --cLight)
            {
                foreach (var pos in _addLightQueue![cLight])
                {
                    if (!n.GetChunk(pos.ShiftRight(Constants.ChunkSizeLog2))!.Data!.HasSkyLight)
                    {
                        n.ScheduleLightUpdate(pos);
                        continue;
                    }
                    if(n.GetLight(pos, channel) >= cLight) continue;
                    n.SetLight(pos, channel, (byte)cLight);
                    if(cLight <= 1) continue;
                    foreach (var face in CubeFaceUtils.Values())
                    {
                        var oPos = pos + face.GetDelta();
                        if(n.GetLight(oPos, channel) >= cLight - 1) continue;
                        var canLightPass = n.GetBlockObj(pos).CanLightLeave(face, channel) &&
                                           n.GetBlockObj(oPos).CanLightEnter(face.GetOpposite(), channel);
                        if (canLightPass)
                        {
                            _addLightQueue[cLight - 1].Add(oPos);
                        }
                    }
                }
                _addLightQueue[cLight].Clear();
            }
        }
    }
}