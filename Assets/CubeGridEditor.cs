using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CubeGridEditor : MonoBehaviour
{
    public GameObject editCamera, editPanel;
    public CubeGrid cubeGrid;
    public EventSystem eventSystem;
    public Material phantomMaterial;
    public TMP_InputField exportInput, importInput;

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
    private GameObject phantomCube;
    private bool isEditing, isEditingSignals;
    private int currentPrefabIndex = 1;
    private Rotation currentRotation = Rotation.NORTH;
    private Vector3Int? currentSignalProducerCoords;
    private Vector3Int? currentSignalConsumerCoords;

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
            if (isEditingSignals)
            {
                if (!eventSystem.IsPointerOverGameObject())
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        SetSignalsToHovered();
                    }
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        //TODO: Unlink if existing link between currentProducer/currentConsumer and coords
                    }
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ClearSignals();
                    }
                }
            }
            else
            {
                if (!eventSystem.IsPointerOverGameObject())
                {
                    ChangeCellToHovered();
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        if (cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
                        {
                            ChangeSelectedElement(currentPrefabIndex);
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        ChangeSelectedElement(0);
                        GeneratePhantom();
                    }
                }
                if (Input.GetKeyDown(KeyCode.E))
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
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    StartExploring();
                }
            }

            if (!eventSystem.IsPointerOverGameObject())
            {
                if (Input.mouseScrollDelta.y != 0)
                {
                    editCameraComponent.orthographicSize = Mathf.Clamp(editCameraComponent.orthographicSize - Input.mouseScrollDelta.y * Time.deltaTime * cameraZoomSpeed, cameraMinZoom, cameraMaxZoom);
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
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartPlaying();
            }
        }
    }

    private void SetSignalsToHovered()
    {
        Vector3Int? cursorCell = GetCursorCell();
        if (cursorCell.HasValue)
        {
            int x = cursorCell.Value.x;
            int y = cursorCell.Value.y;
            int z = cursorCell.Value.z;
            if (x >= 0 && x < cubeGrid.width && y >= 0 && y < cubeGrid.height && z >= 0 && z < cubeGrid.depth)
            {
                SignalProducer producer = cubeGrid.GetInstance(x, y, z).TryGetComponentInChildren<SignalProducer>();
                SignalConsumer consumer = cubeGrid.GetInstance(x, y, z).TryGetComponentInChildren<SignalConsumer>();
                if (currentSignalConsumerCoords.HasValue)
                {
                    if (producer != null)
                    {
                        cubeGrid.AddConsumerToProducer(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z, x, y, z);
                        cubeGrid.ChangeMaterial(x, y, z, k => k.HighlightLinked());
                        cubeGrid.ChangeMaterial(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z, k => k.Select());
                    } else if (consumer != null)
                    {
                        cubeGrid.ChangeMaterial(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z, k => k.HighlightSignal());
                        currentSignalConsumerCoords = new Vector3Int(x, y, z);
                        cubeGrid.ChangeMaterial(x, y, z, k => k.Select());
                        cubeGrid.ChangeAllProducersMaterials(x => x.HighlightUnlinked());
                        cubeGrid.ChangeAllProducersLinkedToConsumerMaterials(x, y, z, k => k.HighlightLinked());
                    }
                    else
                    {
                        ClearSignals();
                    }
                } else if (currentSignalProducerCoords.HasValue)
                {
                    if (consumer != null)
                    {
                        cubeGrid.AddConsumerToProducer(x, y, z, currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z);
                        cubeGrid.ChangeMaterial(x, y, z, k => k.HighlightLinked());
                        cubeGrid.ChangeMaterial(currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z, k => k.Select());
                    }
                    else if (producer != null)
                    {
                        cubeGrid.ChangeMaterial(currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z, k => k.HighlightSignal());
                        currentSignalProducerCoords = new Vector3Int(x, y, z);
                        cubeGrid.ChangeMaterial(x, y, z, k => k.Select());
                        cubeGrid.ChangeAllConsumersMaterials(x => x.HighlightUnlinked());
                        cubeGrid.ChangeAllConsumersLinkedToProducerMaterials(x, y, z, k => k.HighlightLinked());
                    }
                    else
                    {
                        ClearSignals();
                    }
                }
                else
                {
                    if (producer != null)
                    {
                        currentSignalProducerCoords = new Vector3Int(x, y, z);
                        cubeGrid.ChangeMaterial(x, y, z, k => k.Select());
                        cubeGrid.ChangeAllConsumersMaterials(x => x.HighlightUnlinked());
                        cubeGrid.ChangeAllConsumersLinkedToProducerMaterials(x, y, z, k => k.HighlightLinked());
                    }
                    else if (consumer != null)
                    {
                        currentSignalConsumerCoords = new Vector3Int(x, y, z);
                        cubeGrid.ChangeMaterial(x, y, z, k => k.Select());
                        cubeGrid.ChangeAllProducersMaterials(x => x.HighlightUnlinked());
                        cubeGrid.ChangeAllProducersLinkedToConsumerMaterials(x, y, z, k => k.HighlightLinked());
                    }
                    else
                    {
                        ClearSignals();
                    }
                }
            }
        }
    }

    private void ClearSignals()
    {
        currentSignalConsumerCoords = null;
        currentSignalProducerCoords = null;
        cubeGrid.ChangeAllSignalsMaterials(x => x.HighlightSignal());
    }

    private void ChangeCellToHovered()
    {
        Vector3Int? cursorCell = GetCursorCell();
        if (cursorCell.HasValue)
        {
            ChangeCurrentCell(cursorCell.Value.x, cursorCell.Value.y, cursorCell.Value.z);
        }
    }

    private Vector3Int? GetCursorCell()
    {
        Plane plane = new Plane(Vector3.up, Vector3.up * currentY);
        Ray ray = editCameraComponent.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 contactPoint = ray.GetPoint(enter);
            return new Vector3Int(Mathf.RoundToInt(contactPoint.x), currentY, Mathf.RoundToInt(contactPoint.z));
        }
        return null;
    }

    public void ChangeCurrentPrefabIndex(int index)
    {
        currentPrefabIndex = index;
        DestroyPhantom();
        GeneratePhantom();
    }

    public void Export()
    {
        using MemoryStream memoryStream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(memoryStream);
        writer.Write(0);
        cubeGrid.Save(writer);
        exportInput.text = System.Convert.ToBase64String(memoryStream.ToArray());
    }

    public void Import()
    {
        string importString = importInput.text;
        byte[] bytes = System.Convert.FromBase64String(importString);
        using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
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

    public void StartPlaying()
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
        cubeGrid.MakeAllLayersVisible();
        cubeGrid.ChangeAllMaterials(x => x.ResetMaterial());
        isEditing = false;
        editPanel.SetActive(false);
        editCamera.SetActive(false);
        DestroyPhantom();
    }

    public void StartEditingSignals()
    {
        isEditingSignals = true;
        DestroyPhantom();
        cubeGrid.ChangeAllMaterials(x => x.Shadow());
        cubeGrid.ChangeAllSignalsMaterials(x => x.HighlightSignal());
    }

    public void StopEditingSignals()
    {
        isEditingSignals = false;
        GeneratePhantom();
        cubeGrid.ChangeAllMaterials(x => x.ResetMaterial());
    }

    private void GeneratePhantom()
    {
        DestroyPhantom();
        phantomCube = Instantiate(PrefabHelper.PrefabFromIndex(currentPrefabIndex), new Vector3(currentX, currentY, currentZ), currentRotation.ToWorldRot());
        phantomCube.GetComponentInChildren<Renderer>().material = phantomMaterial;
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
                cubeGrid.ChangeMaterial(currentX, currentY, currentZ, x => x.ResetMaterial());
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
                cubeGrid.ChangeMaterial(currentX, currentY, currentZ, x => x.Select());
            }
        }
    }

    public void RotateCurrent()
    {
        cubeGrid.RotateElement(currentX, currentY, currentZ);
        cubeGrid.ChangeMaterial(currentX, currentY, currentZ, x => x.Select());
    }

    public void ChangeSelectedElement(int prefabIndex)
    {
        cubeGrid.ChangeElement(currentX, currentY, currentZ, prefabIndex, currentRotation);
        cubeGrid.ChangeMaterial(currentX, currentY, currentZ, x => x.Select());
    }
}
