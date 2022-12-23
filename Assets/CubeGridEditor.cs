using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGridEditor : MonoBehaviour
{
    public GameObject editCamera;
    public CubeGrid cubeGrid;
    public GameObject phantomPrefab, playerPrefab;

    [SerializeField, Range(0f, 100f)]
    float cameraSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    float cameraZoomSpeed = 50f;
    [SerializeField, Range(0f, 10f)]
    float cameraMaxZoom = 0.2f;
    [SerializeField, Range(0f, 10f)]
    float cameraMinZoom = 5f;

    private Camera editCameraComponent;
    private int currentX, currentY, currentZ;
    private GameObject phantomCube, exploringPlayer;
    private bool isEditing, isExploring;

    private void Awake()
    {
        editCameraComponent = editCamera.GetComponent<Camera>();
        StartEditing();
    }

    private void Update()
    {
        if (isEditing)
        {
            ChangeCellToHovered();
            //TODO: Use layout-insensitive key mappings
            if (Input.GetKey(KeyCode.W))
            {
                editCamera.transform.Translate(cameraSpeed * Time.deltaTime * Vector3.up);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                editCamera.transform.Translate(cameraSpeed * Time.deltaTime * Vector3.right);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                editCamera.transform.Translate(cameraSpeed * Time.deltaTime * Vector3.down);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                editCamera.transform.Translate(cameraSpeed * Time.deltaTime * Vector3.left);
            }
            if (Input.mouseScrollDelta.y != 0)
            {
                editCameraComponent.orthographicSize = Mathf.Clamp(editCameraComponent.orthographicSize - Input.mouseScrollDelta.y * Time.deltaTime * cameraZoomSpeed, cameraMinZoom, cameraMaxZoom);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeCurrentCell(currentX, currentY + 1, currentZ);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeCurrentCell(currentX, currentY - 1, currentZ);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeCurrentPrefab(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeCurrentPrefab(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeCurrentPrefab(2);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RotateCurrent();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopEditing();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                StartExploring();
            }
        }
        else if (isExploring)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StopExploring();
            }
        }
    }

    private void ChangeCellToHovered()
    {
        Plane plane = new Plane(Vector3.up, Vector3.up * currentY);
        Ray ray = editCameraComponent.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 contactPoint = ray.GetPoint(enter);
            ChangeCurrentCell(Mathf.RoundToInt(contactPoint.x), currentY, Mathf.RoundToInt(contactPoint.z));
        }
    }

    public void StartExploring()
    {
        isExploring = true;
        exploringPlayer = Instantiate(playerPrefab, new Vector3(currentX, currentY, currentZ), Quaternion.identity);
        StopEditing();
    }

    public void StopExploring()
    {
        isExploring = false;
        Destroy(exploringPlayer);
        StartEditing();
    }

    public void StartEditing()
    {
        isEditing = true;
        editCamera.SetActive(true);
        phantomCube = Instantiate(phantomPrefab);
    }

    public void StopEditing()
    {
        isEditing = false;
        editCamera.SetActive(false);
        Destroy(phantomCube);
    }

    public void ChangeCurrentCell(int x, int y, int z)
    {
        if(x >= 0 && x < cubeGrid.width && y >= 0 && y < cubeGrid.height && z >= 0 && z < cubeGrid.depth)
        {
            currentX = x;
            currentY = y;
            currentZ = z;
            phantomCube.transform.position = new Vector3(x, y, z);
        }
    }

    public void RotateCurrent()
    {
        cubeGrid.RotateElement(currentX, currentY, currentZ);
    }

    public void ChangeCurrentPrefab(int prefabIndex)
    {
        cubeGrid.ChangeElementPrefab(currentX, currentY, currentZ, prefabIndex);
    }
}
