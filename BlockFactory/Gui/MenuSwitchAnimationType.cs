using BlockFactory.Base;

namespace BlockFactory.Gui;

[ExclusiveTo(Side.Client)]
public enum MenuSwitchAnimationType
{
    None,
    Push,
    Pop
}