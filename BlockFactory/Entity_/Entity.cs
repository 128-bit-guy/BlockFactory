using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Util.Math_;
using BlockFactory.World_;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_;

public class Entity
{
    public GameInstance? GameInstance;
    public Vector2 HeadRotation;
    public long Id;
    public EntityPos Pos;
    public Vector3 PrevPosDelta = Vector3.Zero;
    public double PrevTime = 0;
    public World? World;

    private void UpdatePos()
    {
        foreach (var connection in GameInstance!.NetworkHandler.GetAllConnections())
            connection.SendPacket(new EntityPosUpdatePacket(Pos, Id));
    }

    protected virtual void TickInternal()
    {
    }

    protected virtual void TickPhysics()
    {
    }

    public virtual void OnRemoveFromWorld()
    {
        World = null;
    }

    public void Tick()
    {
        TickInternal();
        TickPhysics();
        if (GameInstance!.Kind.DoesProcessLogic() && GameInstance!.Kind.IsNetworked()) UpdatePos();
    }

    public Vector3 GetForward()
    {
        var x = MathF.Sin(HeadRotation.X) * MathF.Cos(HeadRotation.Y);
        var y = MathF.Sin(HeadRotation.Y);
        var z = MathF.Cos(HeadRotation.X) * MathF.Cos(HeadRotation.Y);
        return (x, y, z);
    }

    public Vector3 GetUp()
    {
        var x = -MathF.Sin(HeadRotation.X) * MathF.Sin(HeadRotation.Y);
        var y = MathF.Cos(HeadRotation.Y);
        var z = -MathF.Cos(HeadRotation.X) * MathF.Sin(HeadRotation.Y);
        return (x, y, z);
    }

    public Vector3 GetRight()
    {
        return Vector3.Cross(GetForward(), GetUp());
    }
}