namespace BlockFactory.Network;

public interface INetworkHandler
{
    void Update();
    bool ShouldStop();
}