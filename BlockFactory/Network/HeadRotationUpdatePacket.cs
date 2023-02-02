using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class HeadRotationUpdatePacket : IInGamePacket
{
    public readonly Vector2 NewRotation;

    public HeadRotationUpdatePacket(BinaryReader reader)
    {
        NewRotation = NetworkUtils.ReadVector2(reader);
    }

    public HeadRotationUpdatePacket(Vector2 newRotation)
    {
        NewRotation = newRotation;
    }

    public void Write(BinaryWriter writer)
    {
        NewRotation.Write(writer);
    }

    public void Process(NetworkConnection connection)
    {
        ((PlayerEntity)connection.SideObject!).HeadRotation = NewRotation;
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend;
    }
}