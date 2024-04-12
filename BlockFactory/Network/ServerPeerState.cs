using System.Collections;
using BlockFactory.Base;
using BlockFactory.Content.Entity_;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Server)]
public class ServerPeerState
{
    public ServerPlayerEntity? Player;
    public IEnumerator PreGameEnumerator = null!;
    public Queue<IPacket> PreGameQueue = new();

    public IEnumerable WaitForPreGamePacket<T>() where T : IPacket
    {
        while (true)
        {
            if (PreGameQueue.Count == 0)
            {
                yield return null;
                continue;
            }

            if (PreGameQueue.Peek() is not T)
            {
                PreGameQueue.Dequeue();
                yield return null;
                continue;
            }

            yield break;
        }
    }
}