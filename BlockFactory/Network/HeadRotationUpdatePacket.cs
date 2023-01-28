using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockFactory.Util;

namespace BlockFactory.Network
{
    public class HeadRotationUpdatePacket : IPacket
    {
        public readonly Vector2 NewRotation;
        public HeadRotationUpdatePacket(BinaryReader reader) {
            NewRotation = NetworkUtils.ReadVector2(reader);
        }
        public HeadRotationUpdatePacket(Vector2 newRotation) {
            NewRotation = newRotation;
        }
        public void Write(BinaryWriter writer)
        {
            NewRotation.Write(writer);
        }
    }
}
