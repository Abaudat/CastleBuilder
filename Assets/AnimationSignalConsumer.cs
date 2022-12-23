using UnityEngine;

public class AnimationSignalConsumer : SignalConsumer
{
    public Animator animator;

    public override void Trigger()
    {
        animator.SetTrigger("trigger");
    }
}
