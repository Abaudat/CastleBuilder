using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System;

public class CubeGridEditor : MonoBehaviour
{
    public GameObject editCamera, editPanel, editLayerPlanePrefab, roof, electricityLinePrefab;
    public Transform electricityLinesParent;
    public CubeGrid cubeGrid;
    public EventSystem eventSystem;
    public Material phantomMaterial;
    public TMP_InputField exportInput, importInput;
    public EditCameraMover editCameraMover;
    public Slider floorSlider;
    public TMP_Text floorText;

    private Camera editCameraComponent;
    private PlayManager playManager;
    private SoundManager soundManager;
    private TutorialManager tutorialManager;
    private int currentX, currentY, currentZ;
    private GameObject phantomCube, editLayerPlane;
    private bool isEditing, isEditingSignals;
    private int currentPrefabIndex = 1;
    private Rotation currentRotation = Rotation.NORTH;
    private Vector3Int? currentSignalProducerCoords;
    private Vector3Int? currentSignalConsumerCoords;

    private void Start()
    {
        playManager = FindObjectOfType<PlayManager>();
        soundManager = FindObjectOfType<SoundManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        editCameraComponent = editCamera.GetComponent<Camera>();
    }

    private void Update()
    {
        if (isEditing)
        {
            if (isEditingSignals)
            {
                if (!eventSystem.IsPointerOverGameObject())
                {
                    Vector3Int? cursorCell = GetCursorCell();
                    if (cursorCell.HasValue)
                    {
                        ChangeCurrentCellSignalMode(cursorCell.Value.x, cursorCell.Value.y, cursorCell.Value.z);
                    }
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        LinkSignals();
                    }
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        UnlinkSignals();
                    }
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ClearSignals();
                        cubeGrid.SetSignalModeMaterials(currentY, currentSignalProducerCoords, currentSignalConsumerCoords, new Vector3Int(currentX, currentY, currentZ));
                    }
                }
            }
            else
            {
                if (!eventSystem.IsPointerOverGameObject())
                {
                    ChangeCellToHovered();
                    if (Input.GetKey(KeyCode.Mouse0))
                    {
                        if (cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
                        {
                            tutorialManager.PlaceBlock();
                            ChangeSelectedElement(currentPrefabIndex);
                            soundManager.PlayBuildSound();
                        }
                    }
                    if (Input.GetKey(KeyCode.Mouse1))
                    {
                        if (!cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
                        {
                            soundManager.PlayDestroySound();
                            ChangeSelectedElement(0);
                            GeneratePhantom();
                        }
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
                    soundManager.PlayRotateSound();
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    StartExploring();
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                tutorialManager.ChangeFloor();
                ChangeLayer(currentY + 1);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                tutorialManager.ChangeFloor();
                ChangeLayer(currentY - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartPlaying();
            }
        }
    }

    private void LinkSignals()
    {
        Vector3Int? cursorCell = GetCursorCell();
        if (cursorCell.HasValue)
        {
            int x = cursorCell.Value.x;
            int y = cursorCell.Value.y;
            int z = cursorCell.Value.z;
            if (x >= 0 && x < cubeGrid.width && y >= 0 && y < cubeGrid.height && z >= 0 && z < cubeGrid.depth)
            {
                GameObject instance = cubeGrid.GetInstance(x, y, z);
                SignalProducer producer = null;
                SignalConsumer consumer = null;
                if (instance != null)
                {
                    producer = instance.TryGetComponentInChildren<SignalProducer>();
                    consumer = instance.TryGetComponentInChildren<SignalConsumer>();
                }
                if (currentSignalConsumerCoords.HasValue)
                {
                    if (producer != null)
                    {
                        bool consumerAdded = cubeGrid.AddConsumerToProducer(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z, x, y, z);
                        if (consumerAdded)
                        {
                            soundManager.PlayElectricityLinkSound();
                        }
                    } else if (consumer != null)
                    {
                        if (!currentSignalConsumerCoords.Value.Equals(new Vector3Int(x, y, z)))
                        {
                            currentSignalConsumerCoords = new Vector3Int(x, y, z);
                            soundManager.PlayElectricitySelectSound();
                        }
                    }
                    else
                    {
                        ClearSignals();
                        soundManager.PlayElectricityUnselectSound();
                    }
                } else if (currentSignalProducerCoords.HasValue)
                {
                    if (consumer != null)
                    {
                        bool consumerAdded = cubeGrid.AddConsumerToProducer(x, y, z, currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z);
                        if (consumerAdded)
                        {
                            soundManager.PlayElectricityLinkSound();
                        }
                    }
                    else if (producer != null)
                    {
                        if (!currentSignalProducerCoords.Value.Equals(new Vector3Int(x, y, z)))
                        {
                            currentSignalProducerCoords = new Vector3Int(x, y, z);
                            soundManager.PlayElectricitySelectSound();
                        }
                    }
                    else
                    {
                        ClearSignals();
                        soundManager.PlayElectricityUnselectSound();
                    }
                }
                else
                {
                    if (producer != null)
                    {
                        currentSignalProducerCoords = new Vector3Int(x, y, z);
                        soundManager.PlayElectricitySelectSound();
                    }
                    else if (consumer != null)
                    {
                        currentSignalConsumerCoords = new Vector3Int(x, y, z);
                        soundManager.PlayElectricitySelectSound();
                    }
                    else
                    {
                        ClearSignals();
                    }
                }
                cubeGrid.SetSignalModeMaterials(currentY, currentSignalProducerCoords, currentSignalConsumerCoords, new Vector3Int(currentX, currentY, currentZ));
                RecreateElectricityLines();
            }
        }
    }

    private void UnlinkSignals()
    {
        Vector3Int? cursorCell = GetCursorCell();
        if (cursorCell.HasValue)
        {
            int x = cursorCell.Value.x;
            int y = cursorCell.Value.y;
            int z = cursorCell.Value.z;
            if (x >= 0 && x < cubeGrid.width && y >= 0 && y < cubeGrid.height && z >= 0 && z < cubeGrid.depth)
            {
                GameObject instance = cubeGrid.GetInstance(x, y, z);
                SignalProducer producer = null;
                SignalConsumer consumer = null;
                if (instance != null)
                {
                    producer = instance.TryGetComponentInChildren<SignalProducer>();
                    consumer = instance.TryGetComponentInChildren<SignalConsumer>();
                }
                if (currentSignalConsumerCoords.HasValue)
                {
                    if (producer != null && cubeGrid.ProducerContainsConsumer(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z, x, y, z))
                    {
                        cubeGrid.RemoveConsumerFromProducer(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z, x, y, z);
                        soundManager.PlayElectricityUnlinkSound();
                    }
                    else
                    {
                        ClearSignals();
                        soundManager.PlayElectricityUnselectSound();
                    }
                }
                else if (currentSignalProducerCoords.HasValue)
                {
                    if (consumer != null && cubeGrid.ProducerContainsConsumer(x, y, z, currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z))
                    {
                        cubeGrid.RemoveConsumerFromProducer(x, y, z, currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z);
                        soundManager.PlayElectricityUnlinkSound();
                    }
                    else
                    {
                        ClearSignals();
                        soundManager.PlayElectricityUnselectSound();
                    }
                }
                else
                {
                    ClearSignals();
                }
                cubeGrid.SetSignalModeMaterials(currentY, currentSignalProducerCoords, currentSignalConsumerCoords, new Vector3Int(currentX, currentY, currentZ));
                RecreateElectricityLines();
            }
        }
    }

    private void RecreateElectricityLines()
    {
        ClearElectricitryLines();
        if (currentSignalConsumerCoords.HasValue)
        {
            foreach (Vector3Int producerCoord in cubeGrid.ProducersForConsumer(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z))
            {
                if (producerCoord.y == currentY || currentSignalConsumerCoords.Value.y == currentY)
                {
                    GameObject electricityLine = Instantiate(electricityLinePrefab, electricityLinesParent);
                    electricityLine.GetComponent<LineRenderer>().SetPosition(0, producerCoord);
                    electricityLine.GetComponent<LineRenderer>().SetPosition(1, new Vector3(currentSignalConsumerCoords.Value.x, currentSignalConsumerCoords.Value.y, currentSignalConsumerCoords.Value.z));
                    if (currentSignalConsumerCoords.Value.y < currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().endWidth = 0.1f / (1 + currentY - currentSignalConsumerCoords.Value.y);
                        electricityLine.GetComponent<LineRenderer>().endColor = new Color(1, 1, 1, 0);
                    }
                    else if (currentSignalConsumerCoords.Value.y > currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().endWidth = 0.1f * (1 + currentSignalConsumerCoords.Value.y - currentY);
                        electricityLine.GetComponent<LineRenderer>().endColor = new Color(1, 1, 1, 0);
                    }
                    if (producerCoord.y < currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().startWidth = 0.1f / (1 + currentY - producerCoord.y);
                        electricityLine.GetComponent<LineRenderer>().startColor = new Color(1, 1, 1, 0);
                    }
                    else if (producerCoord.y > currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().startWidth = 0.1f * (1 + producerCoord.y - currentY);
                        electricityLine.GetComponent<LineRenderer>().startColor = new Color(1, 1, 1, 0);
                    }
                }
            }
        }
        else if (currentSignalProducerCoords.HasValue)
        {
            foreach (Vector3Int consumerCoord in cubeGrid.ConsumersForProducer(currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z))
            {
                if (consumerCoord.y == currentY || currentSignalProducerCoords.Value.y == currentY)
                {
                    GameObject electricityLine = Instantiate(electricityLinePrefab, electricityLinesParent);
                    electricityLine.GetComponent<LineRenderer>().SetPosition(0, new Vector3(currentSignalProducerCoords.Value.x, currentSignalProducerCoords.Value.y, currentSignalProducerCoords.Value.z));
                    electricityLine.GetComponent<LineRenderer>().SetPosition(1, consumerCoord);
                    if (currentSignalProducerCoords.Value.y < currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().startWidth = 0.1f / (1 + currentY - currentSignalProducerCoords.Value.y);
                        electricityLine.GetComponent<LineRenderer>().startColor = new Color(1, 1, 1, 0);
                    }
                    else if (currentSignalProducerCoords.Value.y > currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().startWidth = 0.1f * (1 + currentSignalProducerCoords.Value.y - currentY);
                        electricityLine.GetComponent<LineRenderer>().startColor = new Color(1, 1, 1, 0);
                    }
                    if (consumerCoord.y < currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().endWidth = 0.1f / (1 + currentY - consumerCoord.y);
                        electricityLine.GetComponent<LineRenderer>().endColor = new Color(1, 1, 1, 0);
                    }
                    else if (consumerCoord.y > currentY)
                    {
                        electricityLine.GetComponent<LineRenderer>().endWidth = 0.1f * (1 + consumerCoord.y - currentY);
                        electricityLine.GetComponent<LineRenderer>().endColor = new Color(1, 1, 1, 0);
                    }
                }
            }
        }

    }

    private void ClearElectricitryLines()
    {
        foreach (Transform line in electricityLinesParent) //TODO: Add pooling
        {
            Destroy(line.gameObject);
        }
    }

    private void ClearSignals()
    {
        currentSignalConsumerCoords = null;
        currentSignalProducerCoords = null;
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
        Plane plane = new Plane(Vector3.up, Vector3.up * (currentY - 0.5f));
        Ray ray = editCameraComponent.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 contactPoint = ray.GetPoint(enter);
            return new Vector3Int(Mathf.RoundToInt(contactPoint.x), currentY, Mathf.RoundToInt(contactPoint.z));
        }
        return null;
    }

    public int getCurrentY()
    {
        return currentY;
    }

    public void ClearAll()
    {
        cubeGrid.ClearAll();
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
        cubeGrid.Save(writer);
        byte[] compressedBytes = PersistenceHelpers.compressBytes(memoryStream.ToArray());
        byte[] bytesWithFlag = PersistenceHelpers.addFlag(compressedBytes, 2);
        exportInput.text = PersistenceHelpers.bytesToStringLevelCode(bytesWithFlag);
    }

    public void Import()
    {
        string importString = importInput.text;
        byte[] bytes = PersistenceHelpers.stringLevelCodeToBytes(importString);
        int format = BitConverter.ToInt32(bytes.Take(4).ToArray());
        BinaryReader reader;
        if (format == 0 || format == 1)
        {
            reader = new BinaryReader(new MemoryStream(bytes.Skip(4).ToArray()));
        }
        else if (format == 2)
        {
            reader = new BinaryReader(new MemoryStream(PersistenceHelpers.decompressBytes(bytes.Skip(4).ToArray())));
        }
        else
        {
            Debug.LogWarning("Unknown map format " + format);
            return;
        }
        cubeGrid.Load(reader, format);
        cubeGrid.SetPlacementModeMaterials(currentY);
    }

    public void StartExploring()
    {
        StopEditing();
        playManager.StartExploring(currentX, currentY, currentZ);
    }

    public void StartPlaying()
    {
        if (cubeGrid.ContainsAtLeastOneChest())
        {
            StopEditing();
            playManager.StartValidating();
        }
        else
        {
            tutorialManager.ShowNoChestPanel();
        }
    }

    public void StartEditing()
    {
        isEditing = true;
        editPanel.SetActive(true);
        editCamera.SetActive(true);
        roof.SetActive(false);
        cubeGrid.RecreateAllElements();
        GeneratePhantom();
        editLayerPlane = Instantiate(editLayerPlanePrefab, new Vector3(4.5f, currentY - 0.49f, 4.5f), Quaternion.identity);
        cubeGrid.SetPlacementModeMaterials(currentY);
    }

    public void StopEditing()
    {
        cubeGrid.ChangeAllMaterials(x => x.Reset());
        ClearElectricitryLines();
        isEditing = false;
        editPanel.SetActive(false);
        editCameraMover.MakeAllWallsVisible();
        roof.SetActive(true);
        DestroyPhantom();
        Destroy(editLayerPlane);
    }

    public void StartEditingSignals()
    {
        isEditingSignals = true;
        DestroyPhantom();
        cubeGrid.ChangeAllMaterials(x => x.Shadow());
        cubeGrid.SetSignalModeMaterials(currentY, currentSignalProducerCoords, currentSignalConsumerCoords, new Vector3Int(currentX, currentY, currentZ));
    }

    public void StopEditingSignals()
    {
        isEditingSignals = false;
        currentSignalConsumerCoords = null;
        currentSignalProducerCoords = null;
        GeneratePhantom();
        cubeGrid.SetPlacementModeMaterials(currentY);
        ClearElectricitryLines();
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

    public void ChangeLayer(int newY)
    {
        if (newY >= 0 && newY < cubeGrid.height)
        {
            if (newY > currentY)
            {
                soundManager.PlayLayerUpSound();
            }
            else if (newY < currentY)
            {
                soundManager.PlayLayerDownSound();
            }
            currentY = newY;
            editLayerPlane.transform.position = new Vector3(4.5f, currentY - 0.49f, 4.5f);
            floorSlider.value = newY;
            floorText.text = (newY + 1).ToString();
            if (isEditingSignals)
            {
                cubeGrid.SetSignalModeMaterials(currentY, currentSignalProducerCoords, currentSignalConsumerCoords, new Vector3Int(currentX, currentY, currentZ));
                RecreateElectricityLines();
            }
            else
            {
                ChangeCurrentCell(currentX, currentY, currentZ);
                cubeGrid.SetPlacementModeMaterials(currentY);
            }
        }
    }

    public void ChangeLayer(float newY)
    {
        ChangeLayer(Mathf.FloorToInt(newY+0.5f));
    }

    public void ChangeCurrentCell(int x, int y, int z)
    {
        if(cubeGrid.IsInBounds(x, y, z))
        {
            if (!cubeGrid.IsElementEmpty(currentX, currentY, currentZ))
            {
                cubeGrid.ChangeMaterial(currentX, currentY, currentZ, x => x.Reset());
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

    public void ChangeCurrentCellSignalMode(int x, int y, int z)
    {
        if (cubeGrid.IsInBounds(x, y, z))
        {
            currentX = x;
            currentY = y;
            currentZ = z;
            cubeGrid.SetSignalModeMaterials(currentY, currentSignalProducerCoords, currentSignalConsumerCoords, new Vector3Int(currentX, currentY, currentZ));
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
