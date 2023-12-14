using BlockFactory.Entity_;

namespace BlockFactory.Network;

public interface INetworkHandler : IDisposable
{
    void Update();
    bool ShouldStop();
    void SendPacket<T>(PlayerEntity? player, T packet) where T : class, IPacket;
    void Start();
}