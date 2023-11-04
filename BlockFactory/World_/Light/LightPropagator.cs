using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Light;

public static class LightPropagator
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
    
    private static int GetSupposedLight(IBlockAccess n, Vector3D<int> pos, LightChannel channel)
    {
        int cLight = n.GetBlockObj(pos).GetEmittedLight(channel);
        foreach (var face in CubeFaceUtils.Values())
        {
            var oPos = pos + face.GetDelta();
            var lightCanPass = n.GetBlockObj(pos).CanLightEnter(face) &&
                               n.GetBlockObj(oPos).CanLightLeave(face.GetOpposite());
            if(!lightCanPass) continue;
            var lightFromNeighbor = n.GetLight(oPos, channel) - 1;
            if (lightFromNeighbor > cLight)
            {
                cLight = lightFromNeighbor;
            }
        }

        return cLight;
    }

    public static void ProcessLightUpdates(Chunk c)
    {
        var n = c.Neighbourhood;
        InitThreadStatics();

        foreach (var channel in Channels)
        {
            foreach (var pos in c.ScheduledLightUpdates)
            {
                if (n.GetLight(pos, channel) != GetSupposedLight(n, pos, channel))
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
                    var lightCanPass = (changed || n.GetBlockObj(pos).CanLightLeave(face)) &&
                                       n.GetBlockObj(oPos).CanLightEnter(face.GetOpposite());
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
                    if(n.GetLight(pos, channel) >= cLight) continue;
                    n.SetLight(pos, channel, (byte)cLight);
                    if(cLight <= 1) continue;
                    foreach (var face in CubeFaceUtils.Values())
                    {
                        var oPos = pos + face.GetDelta();
                        var canLightPass = n.GetBlockObj(pos).CanLightLeave(face) &&
                                           n.GetBlockObj(oPos).CanLightEnter(face.GetOpposite());
                        if (canLightPass)
                        {
                            _addLightQueue[cLight - 1].Add(oPos);
                        }
                    }
                }
                _addLightQueue[cLight].Clear();
            }
        }

        foreach (var pos in c.ScheduledLightUpdates)
        {
            c.Data!.SetLightUpdateScheduled(pos, false);
        }

        c.ScheduledLightUpdates.Clear();
    }
}