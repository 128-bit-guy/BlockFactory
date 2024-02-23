using System.Collections;
using BlockFactory.Base;
using BlockFactory.Entity_;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Server)]
public class ServerPeerState
{
    public ServerPlayerEntity? Player;
    public Queue<IPacket> PreGameQueue = new Queue<IPacket>();
    public IEnumerator PreGameEnumerator = null!;

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