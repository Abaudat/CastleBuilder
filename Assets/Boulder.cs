using UnityEngine;

public class Boulder : CollisionBlock
{
    [SerializeField]
    float minKillVelocity = 10f;

    private new Rigidbody rigidbody;

    private new void Awake()
    {
        base.Awake();
        rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnPlayerCollision()
    {
        if (rigidbody.velocity.magnitude >= minKillVelocity)
        {
            playManager.Die();
        }
    }
}
