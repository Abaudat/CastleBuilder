using UnityEngine;

public class EditCameraMover : MonoBehaviour
{
    public Renderer rightWall, aboveWall, leftWall, belowWall;
    public Transform castleCenter;
    public Material transparentMat, wallMat;

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

    void Update()
    {
        float xMoveIntent = Input.GetAxis("Horizontal");
        float yMoveIntent = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Mouse2)) {
            if (previousMousePos.HasValue)
            {
                float xRotateIntent = Mathf.Clamp(transform.rotation.eulerAngles.x - (Input.mousePosition.y - previousMousePos.Value.y) * Time.deltaTime * cameraRotateSpeedX, cameraRotationMin, cameraRotationMax);
                transform.rotation = Quaternion.Euler(xRotateIntent, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                transform.RotateAround(castleCenter.position, Vector3.up, (Input.mousePosition.x - previousMousePos.Value.x) * Time.deltaTime * cameraRotateSpeedY);
            }
            previousMousePos = Input.mousePosition;
        }
        else
        {
            previousMousePos = null;
            if (Input.mousePosition.x + cameraMoveMarginSize > Screen.width)
            {
                xMoveIntent = 1;
            }
            else if (Input.mousePosition.x - cameraMoveMarginSize < 0)
            {
                xMoveIntent = -1;
            }
            if (Input.mousePosition.y + cameraMoveMarginSize > Screen.height)
            {
                yMoveIntent = 1;
            }
            else if (Input.mousePosition.y - cameraMoveMarginSize < 0)
            {
                yMoveIntent = -1;
            }
            float newY = Input.mouseScrollDelta.y * Time.deltaTime * cameraZoomSpeed;
            Vector3 newPos = transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(xMoveIntent, 0, yMoveIntent) * cameraSpeed * Time.deltaTime;
            Vector3 clampedNewPos = new Vector3(
                Mathf.Clamp(newPos.x, cameraRangeBottomLeft.x, cameraRangeTopRight.x),
                Mathf.Clamp(transform.position.y - newY, cameraMinZoom, cameraMaxZoom),
                Mathf.Clamp(newPos.z, cameraRangeBottomLeft.y, cameraRangeTopRight.y));
            transform.position = clampedNewPos;
        }
        SetWallsMaterials();
    }

    public void MakeAllWallsVisible()
    {
        belowWall.material = wallMat;
        rightWall.material = wallMat;
        aboveWall.material = wallMat;
        leftWall.material = wallMat;
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
