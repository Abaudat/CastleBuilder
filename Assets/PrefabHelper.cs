using UnityEngine;

public class PrefabHelper
{
    public static GameObject PrefabFromIndex(int index)
    {
        switch (index)
        {
            case 1:
                return Resources.Load("Cube") as GameObject;
            case 2:
                return Resources.Load("Ramp") as GameObject;
            case 3:
                return Resources.Load("Corner") as GameObject;
            case 4:
                return Resources.Load("Spike") as GameObject;
            case 5:
                return Resources.Load("Chest") as GameObject;
            case 6:
                return Resources.Load("SpikeTrap") as GameObject;
            case 7:
                return Resources.Load("PressurePlate") as GameObject;
            case 8:
                return Resources.Load("WallToCeiling") as GameObject;
            case 9:
                return Resources.Load("CeilingToWall") as GameObject;
            case 10:
                return Resources.Load("WallToFloor") as GameObject;
            case 11:
                return Resources.Load("FloorToWall") as GameObject;
            case 12:
                return Resources.Load("Boulder") as GameObject;
            case 13:
                return Resources.Load("StickyPressurePlate") as GameObject;
            case 14:
                return Resources.Load("InvertedRamp") as GameObject;
            case 15:
                return Resources.Load("InvertedCorner") as GameObject;
            case 16:
                return Resources.Load("WallToWall") as GameObject;
            case 17:
                return Resources.Load("StraightCorner") as GameObject;
            default:
                return null;
        }
    }
}