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
            default:
                return null;
        }
    }
}