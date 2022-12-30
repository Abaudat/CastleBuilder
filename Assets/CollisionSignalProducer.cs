using UnityEngine;

public class CollisionSignalProducer : SignalProducer
{
    private AudioSource audioSource;

    private void Awake()
    {
        TryGetComponent(out audioSource);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TriggerConsumers();
        if (audioSource)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }
}
