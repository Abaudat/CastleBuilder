using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class CubeGridEditor : MonoBehaviour
{
    public GameObject editCamera, editPanel;
    public CubeGrid cubeGrid;
    public EventSystem eventSystem;
    public Material phantomMaterial;

    [SerializeField, Range(0f, 100f)]
    float cameraSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    float cameraZoomSpeed = 50f;
    [SerializeField, Range(0f, 10f)]
    float cameraMaxZoom = 0.2f;
    [SerializeField, Range(0f, 10f)]
    float cameraMinZoom = 5f;

    private Camera editCameraComponent;
    private PlayManager playManager;
    private int currentX, currentY, currentZ;
    private GameObject phantomCube, exploringPlayer;
    private bool isEditing;
    private int currentPrefabIndex = 1;
    private Rotation currentRotation = Rotation.NORTH;

    private void Start()
    {
        playManager = FindObjectOfType<PlayManager>();
        editCameraComponent = editCamera.GetComponent<Camera>();
        StartEditing();
    }

    private void Update()
    {
        if (isEditing)
        {
            if (!eventSystem.IsPointerOverGameObject())
            {
                ChangeCellToHovered();
                if (Input.mouseScrollDelta.y != 0)
                {
                    editCameraComponent.orthographicSize = Mathf.Clamp(editCameraComponent.orthographicSize - Input.mouseScrollDelta.y * Time.deltaTime * cameraZoomSpeed, cameraMinZoom, cameraMaxZoom);
                }
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (cubeGrid.IsElementEmpty(currentX, currentY, currentZ)) {
                        ChangeSelectedElement(currentPrefabIndex);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    ChangeSelectedElement(0);
                    GeneratePhantom();
                }
            }
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
            if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeCurrentCell(currentX, currentY + 1, currentZ);
                cubeGrid.MakeLayerVisible(currentY);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeCurrentCell(currentX, currentY - 1, currentZ);
                cubeGrid.MakeLayersAboveInvisible(currentY);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
                {
                    currentRotation = currentRotation.Rotate();
                    GeneratePhantom();
                }
                else
                {
                    RotateCurrent();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopEditing();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                StartExploring();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartPlaying();
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

    public void ChangeCurrentPrefabIndex(int index)
    {
        currentPrefabIndex = index;
        DestroyPhantom();
        GeneratePhantom();
    }

    public void Save()
    {
        string path = Path.Combine(Application.persistentDataPath, "test.map");
        using BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create));
        writer.Write(0);
        cubeGrid.Save(writer);
    }

    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "test.map");
        using BinaryReader reader = new BinaryReader(File.OpenRead(path));
        int header = reader.ReadInt32();
        if (header == 0)
        {
            cubeGrid.Load(reader);
        }
        else
        {
            Debug.LogWarning("Unknown map format " + header);
        }
    }

    public void StartExploring()
    {
        StopEditing();
        playManager.StartExploring(currentX, currentY, currentZ);
    }

    public void StartPlaying() //TODO: Move to other class
    {
        StopEditing();
        playManager.StartPlaying();
    }

    public void StartEditing()
    {
        isEditing = true;
        editPanel.SetActive(true);
        editCamera.SetActive(true);
        GeneratePhantom();
        cubeGrid.MakeLayersAboveInvisible(currentY);
    }

    public void StopEditing()
    {
        if (!cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
        {
            cubeGrid.UnselectElement(currentX, currentY, currentZ);
        }
        cubeGrid.MakeAllLayersVisible();
        isEditing = false;
        editPanel.SetActive(false);
        editCamera.SetActive(false);
        DestroyPhantom();
    }

    private void GeneratePhantom()
    {
        DestroyPhantom();
        phantomCube = Instantiate(PrefabHelper.PrefabFromIndex(currentPrefabIndex), new Vector3(currentX, currentY, currentZ), currentRotation.ToWorldRot());
        phantomCube.GetComponent<Renderer>().material = phantomMaterial;
    }

    private void DestroyPhantom()
    {
        if (phantomCube != null)
        {
            Destroy(phantomCube);
        }
    }

    public void ChangeCurrentCell(int x, int y, int z)
    {
        if((x != currentX || y != currentY || z != currentZ) && x >= 0 && x < cubeGrid.width && y >= 0 && y < cubeGrid.height && z >= 0 && z < cubeGrid.depth)
        {
            if (!cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
            {
                cubeGrid.UnselectElement(currentX, currentY, currentZ);
            }
            currentX = x;
            currentY = y;
            currentZ = z;
            if (cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
            {
                GeneratePhantom();
            }
            else
            {
                DestroyPhantom();
                cubeGrid.SelectElement(currentX, currentY, currentZ);
            }
        }
    }

    public void RotateCurrent()
    {
        cubeGrid.RotateElement(currentX, currentY, currentZ);
        cubeGrid.SelectElement(currentX, currentY, currentZ);
    }

    public void ChangeSelectedElement(int prefabIndex)
    {
        cubeGrid.ChangeElement(currentX, currentY, currentZ, prefabIndex, currentRotation);
        cubeGrid.SelectElement(currentX, currentY, currentZ);
    }
}
