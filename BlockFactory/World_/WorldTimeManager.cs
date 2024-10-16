using BlockFactory.Base;
using BlockFactory.Network.Packet_;
using BlockFactory.Utils;
using Silk.NET.Maths;

namespace BlockFactory.World_;

//TODO Serialization of time
public class WorldTimeManager
{
    public readonly World World;
    public long WorldTime;
    private long _lastSentTime;
    public virtual long TicksPerDay => Constants.TicksPerSecond * 60 * 20;

    public WorldTimeManager(World world)
    {
        World = world;
    }

    public void Update()
    {
        ++WorldTime;
        if (World.LogicProcessor.LogicalSide == LogicalSide.Server && Math.Abs(WorldTime - _lastSentTime) > 100)
        {
            _lastSentTime = WorldTime;
            foreach (var player in World.LogicProcessor.GetPlayers())
            {
                if (player.World == World)
                {
                    World.LogicProcessor.NetworkHandler.SendPacket(player, new WorldTimeUpdatePacket(WorldTime));
                }
            }
        }
    }

    public long GetDayTime()
    {
        return WorldTime % TicksPerDay;
    }

    public float GetSunAngle()
    {
        return -(float)GetDayTime() / TicksPerDay * 2 * MathF.PI;
    }
    
    public Vector3D<float> GetSunDirection()
    {
        var mat = Matrix4X4.CreateRotationX(GetSunAngle());
        return Vector3D.Transform(new Vector3D<float>(0, 0, 1), mat);
    }

    public virtual float GetDayCoefficient()
    {
        var coefRaw = (BfMathUtils.SoftSign((GetSunDirection().Y + 0.02f) * 10) + 0.9f) / 1.8f;
        return MathF.Min(MathF.Max(coefRaw, 0), 1);
    }
}