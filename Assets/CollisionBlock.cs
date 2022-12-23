using UnityEngine;

public abstract class CollisionBlock : MonoBehaviour
{
    protected PlayManager playManager;

    private void Awake()
    {
        playManager = FindObjectOfType<PlayManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            OnPlayerCollision();
        }
    }

    protected abstract void OnPlayerCollision();
}
