using UnityEngine;

public abstract class CollisionBlock : MonoBehaviour
{
    protected PlayManager playManager;

    protected virtual void Awake()
    {
        playManager = FindObjectOfType<PlayManager>();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            OnPlayerCollision();
        }
    }

    protected abstract void OnPlayerCollision();
}
