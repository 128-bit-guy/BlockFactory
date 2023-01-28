using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Util.Math_;
using BlockFactory.World_;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_;

public class Entity
{
    public long Id;
    public EntityPos Pos;
    public GameInstance? GameInstance;
    public Vector2 HeadRotation;
    public World? World;
    public Vector3 PrevPosDelta = Vector3.Zero;
    public double PrevTime = 0;

    private void UpdatePos() {
        foreach (NetworkConnection connection in GameInstance!.NetworkHandler.GetAllConnections()) {
            connection.SendPacket(new EntityPosUpdatePacket(Pos, Id));
        }
    }

    protected virtual void TickInternal() { 
        
    }

    protected virtual void TickPhysics()
    {
        
    }

    public virtual void OnRemoveFromWorld() {
        World = null;
    }
    public void Tick() {
        TickInternal();
        TickPhysics();
        if (GameInstance!.Kind.DoesProcessLogic() && GameInstance!.Kind.IsNetworked()) {
            UpdatePos();
        }
    }
    public Vector3 GetForward() {
        float x = MathF.Sin(HeadRotation.X) * MathF.Cos(HeadRotation.Y);
        float y = MathF.Sin(HeadRotation.Y);
        float z = MathF.Cos(HeadRotation.X) * MathF.Cos(HeadRotation.Y);
        return (x, y, z);
    }

    public Vector3 GetUp() {
        float x = -MathF.Sin(HeadRotation.X) * MathF.Sin(HeadRotation.Y);
        float y = MathF.Cos(HeadRotation.Y);
        float z = -MathF.Cos(HeadRotation.X) * MathF.Sin(HeadRotation.Y);
        return (x, y, z);
    }

    public Vector3 GetRight() { 
        return Vector3.Cross(GetForward(), GetUp());
    }
}