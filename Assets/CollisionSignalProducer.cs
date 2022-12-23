using UnityEngine;

public class CollisionSignalProducer : SignalProducer
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Click");
        TriggerConsumers();
    }
}
