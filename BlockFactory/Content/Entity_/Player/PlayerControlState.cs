namespace BlockFactory.Content.Entity_.Player;

[Flags]
public enum PlayerControlState
{
    MovingRight = 1 << 0,
    MovingLeft = 1 << 1,
    MovingUp = 1 << 2,
    MovingDown = 1 << 3,
    MovingForward = 1 << 4,
    MovingBackwards = 1 << 5,
    Sprinting = 1 << 6,
    Using = 1 << 7,
    Attacking = 1 << 8,
    Dropping = 1 << 9
}