namespace BlockFactory.Entity_.Player;

[Flags]
public enum MotionState
{
    MovingForward = 1,
    MovingBackwards = 2,
    MovingLeft = 4,
    MovingRight = 8,
    MovingUp = 16,
    MovingDown = 32,
    Attacking = 64,
    Using = 128
}