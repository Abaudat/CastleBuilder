using UnityEngine;

public class RigidbodyEnabler : MonoBehaviour
{
    [SerializeField]
    bool disabledOnAwake = true;
    new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (disabledOnAwake)
        {
            Disable();
        }
    }

    public void Enable()
    {
        rigidbody.detectCollisions = true;
        rigidbody.useGravity = true;
    }

    public void Disable()
    {
        rigidbody.detectCollisions = false;
        rigidbody.useGravity = false;
    }
}
