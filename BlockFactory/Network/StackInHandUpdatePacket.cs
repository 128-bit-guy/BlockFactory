using BlockFactory.Client;
using BlockFactory.Game;
using BlockFactory.Item_;

namespace BlockFactory.Network;

public class StackInHandUpdatePacket : IInGamePacket
{
    private readonly long _entityId;
    private readonly ItemStack _stack;
    public StackInHandUpdatePacket(BinaryReader reader)
    {
        _entityId = reader.ReadInt64();
        _stack = new ItemStack(reader);
    }

    public StackInHandUpdatePacket(long entityId, ItemStack stack)
    {
        _entityId = entityId;
        _stack = stack;
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write(_entityId);
        _stack.Write(writer);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }

    public void Process(NetworkConnection connection)
    {
        if (BlockFactoryClient.Instance.Player!.Id == _entityId)
        {
            BlockFactoryClient.Instance.Player.StackInHand = _stack;
        }
    }
}