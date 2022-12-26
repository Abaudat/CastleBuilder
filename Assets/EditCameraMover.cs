using UnityEngine;

public class EditCameraMover : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float cameraSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    float cameraZoomSpeed = 50f;
    [SerializeField, Range(0f, 10f)]
    float cameraMaxZoom = 0.2f;
    [SerializeField, Range(0f, 10f)]
    float cameraMinZoom = 5f;
    [SerializeField]
    Vector2 cameraRangeBottomLeft, cameraRangeTopRight;
    [SerializeField]
    float cameraMoveMarginSize = 10f;

    void Update()
    {
        float xMoveIntent = Input.GetAxis("Horizontal");
        float yMoveIntent = Input.GetAxis("Vertical");
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
}
