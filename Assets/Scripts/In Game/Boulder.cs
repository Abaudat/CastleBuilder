using UnityEngine;

public class Boulder : CollisionBlock
{
    public AudioClip boulderCollisionClip, boulderCrushClip;
    public AudioSource rollAudioSource, collisionAudioSource;

    [SerializeField]
    float minKillVelocity = 10f;

    private new Rigidbody rigidbody;
    private bool enteredOrStayedCollision = false;
    private bool preventOtherClip = false;

    protected override void Awake()
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
                rollAudioSource.volume = (rigidbody.velocity.magnitude - minKillVelocity) / (3 * minKillVelocity);
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
            preventOtherClip = true;
            collisionAudioSource.Stop();
            collisionAudioSource.volume = 1;
            collisionAudioSource.clip = boulderCrushClip;
            collisionAudioSource.Play();
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        enteredOrStayedCollision = true;
        if (!collisionAudioSource.isPlaying || !preventOtherClip)
        {
            collisionAudioSource.volume = (Vector3.Dot(collision.impulse, rigidbody.velocity) - minKillVelocity) / (3 * minKillVelocity);
            collisionAudioSource.clip = boulderCollisionClip;
            collisionAudioSource.Play();
        }
        base.OnCollisionEnter(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        enteredOrStayedCollision = true;
    }
}
