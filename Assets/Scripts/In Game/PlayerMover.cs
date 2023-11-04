using UnityEngine;

public class PlayerMover : MonoBehaviour
{
	public Camera playerCamera;
	public bool canMove = true;

	private OptionsManager optionsManager;

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;
	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;
	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;
	[SerializeField, Range(0f, 90f)]
	float maxGroundAngle = 50f;
	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;
	[SerializeField, Range(0f, 100f)]
	float sensitivityX = 100f;
	[SerializeField, Range(0f, 100f)]
	float sensitivityY = 100f;
	[SerializeField, Range(0f, 1f)]
	float gravityStrength = 0.1f;
	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	Rigidbody body;
	Vector3 velocity, desiredVelocity, contactNormal;
	bool desiredJump;
	bool onGround;
	float minGroundDotProduct;
	int stepsSinceLastGrounded, stepsSinceLastJump;

	void Awake()
	{
		optionsManager = FindObjectOfType<OptionsManager>();
		body = GetComponent<Rigidbody>();
		OnValidate();
	}

    private void Start()
    {
		sensitivityX = optionsManager.GetMouseSens();
		sensitivityY = optionsManager.GetMouseSens();
	}

    void Update()
	{
		if (canMove)
        {
			Vector3 playerInput = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
			playerInput = Vector3.ClampMagnitude(playerInput, 1f);
			desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.z) * maxSpeed;
			desiredJump |= Input.GetButtonDown("Jump");
			RotateCamera();
		}
        else
        {
			desiredVelocity = Vector3.zero;
			desiredJump = false;
        }
	}

	void RotateCamera()
    {
		float rotationY = Input.GetAxis("Mouse X") * sensitivityY;
		float newRotX = playerCamera.transform.eulerAngles.x - Input.GetAxis("Mouse Y") * sensitivityX;
		if (newRotX > 80 && newRotX < 180)
        {
			newRotX = 80;
        }
		else if (newRotX < 280 && newRotX > 180)
		{
			newRotX = 280;
		}
		playerCamera.transform.eulerAngles = new Vector3(newRotX, playerCamera.transform.eulerAngles.y, playerCamera.transform.eulerAngles.z);
		transform.Rotate(new Vector3(0, rotationY, 0));
	}

    void FixedUpdate()
	{
		velocity = body.velocity;
		if (onGround || SnapToGround())
		{
			stepsSinceLastGrounded = 0;
			contactNormal.Normalize();
		}
		else
		{
			contactNormal = Vector3.up;
		}
		AdjustVelocity();
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		if (desiredJump)
		{
			desiredJump = false;
			Jump();
		}
		body.velocity = velocity;
		ClearState();
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}

	bool SnapToGround()
	{
		if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
		{
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed)
		{
			return false;
		}
		if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance))
		{
			return false;
		}
		if (hit.normal.y < minGroundDotProduct)
		{
			return false;
		}
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f)
		{
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		return true;
	}

	void ClearState()
	{
		onGround = false;
		contactNormal = Vector3.zero;
	}

	void AdjustVelocity()
	{
		Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
		Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

		float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

		float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
		velocity += -contactNormal * Physics.gravity.magnitude * gravityStrength;
	}

	Vector3 ProjectOnContactPlane(Vector3 vector)
	{
		return vector - contactNormal * Vector3.Dot(vector, contactNormal);
	}

	void EvaluateCollision(Collision collision)
	{
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			if (normal.y >= minGroundDotProduct)
			{
				onGround = true;
				contactNormal += normal;
			}
		}
	}

	void Jump()
	{
        if (onGround)
        {
			stepsSinceLastJump = 0;
			velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
		}
	}
}
