using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SignalProducer : MonoBehaviour
{
    public List<SignalConsumer> consumers;

    public void TriggerConsumers()
    {
        consumers.ForEach(consumer => consumer.Trigger());
    }
}
