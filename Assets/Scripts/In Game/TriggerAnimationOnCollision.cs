using UnityEngine;

public class TriggerAnimationOnCollision : MonoBehaviour
{
    public Animator animator;

    private void OnCollisionEnter(Collision collision)
    {
        animator.SetTrigger("trigger");
    }
}
