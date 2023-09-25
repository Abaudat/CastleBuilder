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

    public static void EnableRigidbody(this GameObject gameObject)
    {
        RigidbodyEnabler rigidbodyEnabler = gameObject.TryGetComponentInChildren<RigidbodyEnabler>();
        if (rigidbodyEnabler != null)
        {
            rigidbodyEnabler.Enable();
        }
    }
}
