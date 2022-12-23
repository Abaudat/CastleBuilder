using UnityEngine;

public class CollisionSignalProducer : SignalProducer
{
    private void OnCollisionEnter(Collision collision)
    {
        TriggerConsumers();
    }
}
