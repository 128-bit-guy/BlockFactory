using System.Runtime.CompilerServices;
using BlockFactory.Base;

namespace BlockFactory.World_.Serialization;

public unsafe struct ChunkArray16
{
    public fixed short Array[Constants.ChunkArrayLength];

    public short this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array[i];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Array[i] = value;
    }
}