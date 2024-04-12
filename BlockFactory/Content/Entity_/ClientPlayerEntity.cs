using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Gui;

namespace BlockFactory.Content.Entity_;

[ExclusiveTo(Side.Client)]
public class ClientPlayerEntity : PlayerEntity
{
    public override MenuManager MenuManager => BlockFactoryClient.MenuManager;
}