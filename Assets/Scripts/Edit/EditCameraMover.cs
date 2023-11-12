using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class EditCameraMover : MonoBehaviour
{
    public Renderer rightWall, aboveWall, leftWall, belowWall;
    public Transform castleCenter;
    public Material transparentMat, wallMat;
    public EventSystem eventSystem;

    private EditLayerManager editLayerManager;

    [SerializeField, Range(0f, 100f)]
    float cameraSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    float cameraZoomSpeed = 50f;
    [SerializeField, Range(0f, 10f)]
    float cameraMaxZoom = 0.2f;
    [SerializeField, Range(0f, 10f)]
    float cameraMinZoom = 5f;
    [SerializeField, Range(0f, 100f)]
    float cameraRotateSpeedX = 10f;
    [SerializeField, Range(0f, 100f)]
    float cameraRotateSpeedY = 10f;
    [SerializeField]
    Vector2 cameraRangeBottomLeft, cameraRangeTopRight;
    [SerializeField]
    float cameraRotationMin, cameraRotationMax;
    [SerializeField]
    float cameraMoveMarginSize = 10f;

    Vector2? previousMousePos;
    Vector3 rotationPivot;

    private float xMoveIntent = 0;
    private float yMoveIntent = 0;
    private bool rotateCamera = false;

    private void Awake()
    {
        editLayerManager = FindObjectOfType<EditLayerManager>();
    }

    void Update()
    {
        if (rotateCamera) {
            if (previousMousePos.HasValue)
            {
                float xRotAngle = -(Mouse.current.position.ReadValue().y - previousMousePos.Value.y) * Time.deltaTime * cameraRotateSpeedX;
                if (transform.eulerAngles.x + xRotAngle >= cameraRotationMin && transform.eulerAngles.x + xRotAngle <= cameraRotationMax)
                {
                    transform.RotateAround(rotationPivot, transform.right, xRotAngle);
                }
                transform.RotateAround(rotationPivot, Vector3.up, (Mouse.current.position.ReadValue().x - previousMousePos.Value.x) * Time.deltaTime * cameraRotateSpeedY);
                transform.rotation = Quaternion.Euler(
                    Mathf.Clamp((transform.eulerAngles.x + 360) % 360, cameraRotationMin, cameraRotationMax), 
                    transform.eulerAngles.y,
                    transform.eulerAngles.z);
            }
            else
            {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
                Plane plane = new Plane(Vector3.up, Vector3.up * editLayerManager.currentHeight);
                plane.Raycast(ray, out float enter);
                rotationPivot = ray.GetPoint(enter);
            }
            previousMousePos = Mouse.current.position.ReadValue();
        }
        else
        {
            previousMousePos = null;
            float mediatedXMoveIntent = xMoveIntent;
            float mediatedYMoveIntent = yMoveIntent;
            if (Mouse.current.position.ReadValue().x + cameraMoveMarginSize > Screen.width)
            {
                mediatedXMoveIntent = 1;
            }
            else if (Mouse.current.position.ReadValue().x - cameraMoveMarginSize < 0)
            {
                mediatedXMoveIntent = -1;
            }
            if (Mouse.current.position.ReadValue().y + cameraMoveMarginSize > Screen.height)
            {
                mediatedYMoveIntent = 1;
            }
            else if (Mouse.current.position.ReadValue().y - cameraMoveMarginSize < 0)
            {
                mediatedYMoveIntent = -1;
            }
            float newY = 0;
            if (!eventSystem.IsPointerOverGameObject())
            {
                newY = Mouse.current.scroll.ReadValue().y * Time.deltaTime * cameraZoomSpeed;
            }
            Vector3 newPos = transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(mediatedXMoveIntent, 0, mediatedYMoveIntent) * cameraSpeed * Time.deltaTime;
            Vector3 clampedNewPos = new Vector3(
                Mathf.Clamp(newPos.x, cameraRangeBottomLeft.x, cameraRangeTopRight.x),
                Mathf.Clamp(transform.position.y - newY, cameraMinZoom, cameraMaxZoom),
                Mathf.Clamp(newPos.z, cameraRangeBottomLeft.y, cameraRangeTopRight.y));
            transform.position = clampedNewPos;
        }
        SetWallsMaterials();
    }

    public void OnMoveInput(CallbackContext callbackContext)
    {
        Vector2 value = callbackContext.ReadValue<Vector2>();
        xMoveIntent = value.x;
        yMoveIntent = value.y;
    }

    public void OnRotateCameraInput(CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            rotateCamera = true;
        }
        else if (callbackContext.canceled)
        {
            rotateCamera = false;
        }
    }

    void SetWallsMaterials()
    {
        float yRot = (transform.rotation.eulerAngles.y + 360) % 360;
        if (yRot >= 0 && yRot < 90)
        {
            belowWall.material = transparentMat;
            rightWall.material = wallMat;
            aboveWall.material = wallMat;
            leftWall.material = transparentMat;
        }
        else if (yRot >= 90 && yRot < 180)
        {
            belowWall.material = wallMat;
            rightWall.material = wallMat;
            aboveWall.material = transparentMat;
            leftWall.material = transparentMat;
        }
        else if (yRot >= 180 && yRot < 270)
        {
            belowWall.material = wallMat;
            rightWall.material = transparentMat;
            aboveWall.material = transparentMat;
            leftWall.material = wallMat;
        }
        else if (yRot >= 270 && yRot < 360)
        {
            belowWall.material = transparentMat;
            rightWall.material = transparentMat;
            aboveWall.material = wallMat;
            leftWall.material = wallMat;
        }
    }
}
