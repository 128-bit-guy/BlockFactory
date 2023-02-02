using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class HeadRotationUpdatePacket : IPacket
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
}