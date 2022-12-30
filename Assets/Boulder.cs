using UnityEngine;

public class Boulder : CollisionBlock
{
    public AudioClip boulderCollisionClip, boulderCrushClip;
    public AudioSource rollAudioSource, collisionAudioSource;

    [SerializeField]
    float minKillVelocity = 10f;

    private new Rigidbody rigidbody;
    private bool enteredOrStayedCollision = false;

    private new void Awake()
    {
        base.Awake();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (enteredOrStayedCollision)
        {
            if (!rollAudioSource.isPlaying)
            {
                rollAudioSource.volume = rigidbody.velocity.magnitude / minKillVelocity;
                rollAudioSource.Play();
            }
        }
        enteredOrStayedCollision = false;
    }

    protected override void OnPlayerCollision()
    {
        if (rigidbody.velocity.magnitude >= minKillVelocity)
        {
            playManager.Die();
            collisionAudioSource.Stop();
            collisionAudioSource.volume = 1;
            collisionAudioSource.clip = boulderCrushClip;
            collisionAudioSource.Play();
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        enteredOrStayedCollision = true;
        Debug.Log("Collision: " + collision.impulse);
        Debug.Log("Collision dot: " + Vector3.Dot(collision.impulse, rigidbody.velocity));
        collisionAudioSource.volume = (Vector3.Dot(collision.impulse, rigidbody.velocity) - minKillVelocity) / (3 * minKillVelocity);
        collisionAudioSource.clip = boulderCollisionClip;
        collisionAudioSource.Play();
        base.OnCollisionEnter(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        enteredOrStayedCollision = true;
    }
}
