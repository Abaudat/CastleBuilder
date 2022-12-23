using System;
using UnityEngine;

public static class GameObjectExtensions
{
    public static T TryGetComponentInChildren<T>(this GameObject gameObject)
    {
        try
        {
            return gameObject.GetComponentInChildren<T>();
        }
        catch (NullReferenceException)
        {
            return default;
        }
    }
}
