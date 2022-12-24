using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SignalProducer : MonoBehaviour
{
    public List<SignalConsumer> consumers;

    [SerializeField, Range(0f, 10f)]
    float reactivationDelay = 0;

    private float timeSinceLastActivation = 0;

    private void Update()
    {
        timeSinceLastActivation += Time.deltaTime;
    }

    public void TriggerConsumers()
    {
        if (timeSinceLastActivation >= reactivationDelay)
        {
            timeSinceLastActivation = 0;
            consumers.ForEach(consumer => consumer.Trigger());
        }
    }
}
