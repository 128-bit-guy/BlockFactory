using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Gui;

namespace BlockFactory.Entity_;

[ExclusiveTo(Side.Client)]
public class ClientPlayerEntity : PlayerEntity
{
    public override MenuManager MenuManager => BlockFactoryClient.MenuManager;
}