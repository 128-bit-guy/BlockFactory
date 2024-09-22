using System.Runtime.CompilerServices;
using BlockFactory.Base;

namespace BlockFactory.World_.Serialization;

public unsafe struct ChunkLightArray
{
    public fixed byte Array[3 * Constants.ChunkArrayLength];
    public byte this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array[i];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Array[i] = value;
    }
}