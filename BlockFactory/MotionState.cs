namespace BlockFactory;

public struct MotionState : IEquatable<MotionState>
{
    public bool MovingForward, MovingBackwards, MovingLeft, MovingRight, MovingUp, MovingDown, Attacking, Using;

    public MotionState(BinaryReader reader)
    {
        var x = reader.ReadByte();
        MovingForward = (x & 1) != 0;
        MovingBackwards = (x & 2) != 0;
        MovingLeft = (x & 4) != 0;
        MovingRight = (x & 8) != 0;
        MovingUp = (x & 16) != 0;
        MovingDown = (x & 32) != 0;
        Attacking = (x & 64) != 0;
        Using = (x & 128) != 0;
    }

    public readonly void Write(BinaryWriter writer)
    {
        byte res = 0;
        if (MovingForward) res |= 1;
        if (MovingBackwards) res |= 2;
        if (MovingLeft) res |= 4;
        if (MovingRight) res |= 8;
        if (MovingUp) res |= 16;
        if (MovingDown) res |= 32;
        if (Attacking) res |= 64;
        if (Using) res |= 128;
        writer.Write(res);
    }

    public bool Equals(MotionState other)
    {
        return MovingForward == other.MovingForward &&
               MovingBackwards == other.MovingBackwards &&
               MovingLeft == other.MovingLeft &&
               MovingRight == other.MovingRight &&
               MovingUp == other.MovingUp &&
               MovingDown == other.MovingDown &&
               Attacking == other.Attacking &&
               Using == other.Using;
    }

    public override bool Equals(object? obj)
    {
        return obj is MotionState other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MovingForward,
            MovingBackwards,
            MovingLeft,
            MovingRight,
            MovingUp,
            MovingDown,
            Attacking,
            Using);
    }

    public static bool operator ==(MotionState left, MotionState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MotionState left, MotionState right)
    {
        return !left.Equals(right);
    }
}