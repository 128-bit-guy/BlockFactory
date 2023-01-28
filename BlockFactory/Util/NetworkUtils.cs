using OpenTK.Mathematics;

namespace BlockFactory.Util
{
    public static class NetworkUtils
    {
        public static Vector3 ReadVector3(BinaryReader reader) { 
            float a = reader.ReadSingle();
            float b = reader.ReadSingle();
            float c = reader.ReadSingle();
            return new Vector3(a, b, c);
        }

        public static void Write(this Vector3 vector, BinaryWriter writer) {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }
        public static Vector3i ReadVector3i(BinaryReader reader) { 
            int a = reader.ReadInt32();
            int b = reader.ReadInt32();
            int c = reader.ReadInt32();
            return new Vector3i(a, b, c);
        }

        public static void Write(this Vector3i vector, BinaryWriter writer) {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }
        public static Vector2 ReadVector2(BinaryReader reader)
        {
            float a = reader.ReadSingle();
            float b = reader.ReadSingle();
            return new Vector2(a, b);
        }

        public static void Write(this Vector2 vector, BinaryWriter writer)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }
        public static Vector2i ReadVector2i(BinaryReader reader) { 
            int a = reader.ReadInt32();
            int b = reader.ReadInt32();
            return new Vector2i(a, b);
        }

        public static void Write(this Vector2i vector, BinaryWriter writer) {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }
    }
}
