using UnityEngine;

public class SpikeTrap : CollisionBlock
{
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out audioSource);
    }

    protected override void OnPlayerCollision()
    {
        audioSource.Play();
        playManager.Die();
    }
}
