using System.Collections;
using System.Net;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Network.Packet_;
using BlockFactory.Registry_;
using ENet.Managed;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Client)]
public class ClientNetworkHandler : MultiPlayerNetworkHandler
{
    private readonly ENetPeer _peer;
    private readonly Queue<IPacket> _preGameQueue = new();
    private bool _connected;
    private bool _isInGame;
    private IEnumerator _preGameEnumerator;

    public ClientNetworkHandler(IPEndPoint remote) : base(LogicalSide.Client,
        new ENetHost(null, 1, 1))
    {
        _peer = Host.Connect(remote, 1, 0);
        _connected = true;
    }

    protected override void OnPeerConnected(ENetPeer peer)
    {
        if (peer != _peer) peer.Disconnect(0);
        _preGameEnumerator = GetPreGameEnumerator();
        _preGameEnumerator.MoveNext();
    }

    protected override void OnPeerDisconnected(ENetPeer peer)
    {
        if (peer == _peer) _connected = false;
    }

    protected override void OnPacketReceived(IPacket packet, ENetPeer peer)
    {
        try
        {
            if (_isInGame || packet is KickPacket)
            {
                if (packet is IInGamePacket p) p.Handle(null);
            }
            else
            {
                _preGameQueue.Enqueue(packet);
                _preGameEnumerator.MoveNext();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private IEnumerable WaitForPreGamePacket<T>() where T : IPacket
    {
        while (true)
        {
            if (_preGameQueue.Count == 0)
            {
                yield return null;
                continue;
            }

            if (_preGameQueue.Peek() is not T)
            {
                _preGameQueue.Dequeue();
                yield return null;
                continue;
            }

            yield break;
        }
    }

    private IEnumerator GetPreGameEnumerator()
    {
        SendPacket(null, new CredentialsPacket(BlockFactoryClient.Settings.Credentials));
        foreach (var _ in WaitForPreGamePacket<RegistryMappingPacket>()) yield return null;
        var p = (RegistryMappingPacket)_preGameQueue.Dequeue();
        SynchronizedRegistries.LoadMapping(p.Mapping);
        _isInGame = true;
    }

    public override bool ShouldStop()
    {
        return !_connected;
    }

    public override void SendPacket<T>(PlayerEntity? player, T packet)
    {
        EnqueueSendPacketInternal(packet, _peer);
    }

    protected override bool ShouldProcessPackets()
    {
        return BlockFactoryClient.LogicProcessor != null;
    }
}