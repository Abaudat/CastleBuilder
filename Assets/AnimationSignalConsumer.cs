using UnityEngine;

public class AnimationSignalConsumer : SignalConsumer
{
    public Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        TryGetComponent(out audioSource);
    }

    public override void Trigger()
    {
        animator.SetTrigger("trigger");
        if (audioSource)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }
}
