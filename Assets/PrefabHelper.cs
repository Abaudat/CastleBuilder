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
            default:
                return null;
        }
    }
}