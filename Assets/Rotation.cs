using UnityEngine;

public enum Rotation
{
    NORTH,
    WEST,
    SOUTH,
    EAST
}

public static class RotationExtensions
{
    public static Quaternion ToWorldRot(this Rotation rotation)
    {
        return Quaternion.Euler(0, 90f * (int)rotation, 0);
    }

    public static Rotation Rotate(this Rotation rotation)
    {
        return (Rotation)(((int)rotation + 1) % 4);
    }
}
