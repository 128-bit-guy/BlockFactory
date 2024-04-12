using BlockFactory.Base;

namespace BlockFactory.Content.Gui;

[ExclusiveTo(Side.Client)]
public enum MenuSwitchAnimationType
{
    None,
    Push,
    Pop
}