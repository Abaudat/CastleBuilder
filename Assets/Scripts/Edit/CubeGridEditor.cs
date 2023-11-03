using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CubeGridEditor : MonoBehaviour
{
    public static event EventHandler<HoveredCellChangedEventArgs> HoveredCellChanged;
    public static event EventHandler EditModeStarted;
    public static event EventHandler EditModeStopped;
    public static event EventHandler FreeEditModeStarted;
    public static event EventHandler<ElementEventArgs> SelectEditModeStarted;
    public static event EventHandler<SignalEditModeStartedEventArgs> SignalEditModeStarted;
    public static event EventHandler CurrentRotationChanged;
    public static event EventHandler CurrentPrefabIndexChanged;

    public EventSystem eventSystem;
    public Camera editCameraComponent;
    public Vector3Int currentHoveredCell;
    public Vector3Int currentSignalTarget, currentSelectedCell;
    public SignalTargetType currentSignalTargetType;

    public int currentPrefabIndex;
    public Rotation currentRotation = Rotation.NORTH;
    public EditMode currentEditMode = EditMode.FREE;
    public bool isEditing;

    private CubeGridInstanceManager cubeGridInstanceCreator;
    private CubeGrid cubeGrid;
    private SoundManager soundManager;
    private CubeGridUndoRedo cubeGridUndoRedo;

    private bool rightClickExitTriggered = false;

    private void Awake()
    {
        cubeGridInstanceCreator = FindObjectOfType<CubeGridInstanceManager>();
        cubeGrid = FindObjectOfType<CubeGrid>();
        soundManager = FindObjectOfType<SoundManager>();
        cubeGridUndoRedo = FindObjectOfType<CubeGridUndoRedo>();
    }

    private void Start()
    {
        EditLayerManager.LayerChanged += LayerChangedHandler;
        CubeGrid.ElementReplaced += ElementChangedHandler;
    }

    private void Update()
    {
        //TODO: Only prevent creation/deletion if over UI, allow right click to back out of modes and other keys
        if (isEditing && !eventSystem.IsPointerOverGameObject())
        {
            ComputeHoveredCell(currentHoveredCell.y);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                        LeftClickSignalMode();
                        break;
                    case EditMode.FREE:
                        LeftClickDownEditMode();
                        break;
                    case EditMode.SELECT:
                        // TODO: Allow selecting another
                        break;
                }
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                        break;
                    case EditMode.SELECT:
                        break;
                    case EditMode.FREE:
                        LeftClickHeldEditMode();
                        break;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                        soundManager.PlayCancelClip();
                        rightClickExitTriggered = true;
                        SetFreeEditMode(); //TODO: Set a flag on getKeyDown, use it in GetKey so we don't back out of mode then immediately delete a block
                        break;
                    case EditMode.FREE:
                        RightClickEditMode();
                        break;
                }
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                        break;
                    case EditMode.FREE:
                        if (!rightClickExitTriggered)
                        {
                            RightClickEditMode();
                        }
                        break;
                }
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                rightClickExitTriggered = false;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                switch (currentEditMode)
                {
                    case EditMode.FREE:
                        RotateFreeMode();
                        break;
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                        RotateSelectMode();
                        break;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (currentEditMode)
                {
                    case EditMode.FREE:
                        break;
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                        SetFreeEditMode();
                        break;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Y)) // TODO: Align with an input manager, this will be the wrong way around in other keyboard locales
            {
                cubeGridUndoRedo.Undo();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                cubeGridUndoRedo.Redo();
            }
        }
    }

    public void StartEditing()
    {
        isEditing = true;
        SetFreeEditMode();
        OnEditModeStarted(EventArgs.Empty);
    }

    public void StopEditing()
    {
        isEditing = false;
        OnEditModeStopped(EventArgs.Empty);
    }

    public void ChangeCurrentPrefabIndex(int newPrefabIndex)
    {
        currentPrefabIndex = newPrefabIndex;
        OnCurrentPrefabIndexChanged(EventArgs.Empty);
        SetFreeEditMode();
    }

    public void SetFreeEditMode()
    {
        currentEditMode = EditMode.FREE;
        OnFreeEditModeStarted(EventArgs.Empty);
    }

    public void StartSignalEditModeForSelectedElement()
    {
        if (cubeGridInstanceCreator.IsElementSignalProducer(currentSelectedCell.x, currentSelectedCell.y, currentSelectedCell.z))
        {
            currentSignalTargetType = SignalTargetType.PRODUCER;
        }
        else if (cubeGridInstanceCreator.IsElementSignalConsumer(currentSelectedCell.x, currentSelectedCell.y, currentSelectedCell.z))
        {
            currentSignalTargetType = SignalTargetType.CONSUMER;
        }
        else
        {
            Debug.Log("Neither a producer nor a consumer!");
            return;
        }
        currentSignalTarget = currentSelectedCell;
        currentEditMode = EditMode.SIGNAL;
        OnSignalEditModeStarted(new(currentSelectedCell.x, currentSelectedCell.y, currentSelectedCell.z, currentSignalTargetType));
    }

    private void ComputeHoveredCell(int y)
    {
        Vector3Int? cellAtCursor = GetCellAtCursor(y);
        if (cellAtCursor.HasValue && !cellAtCursor.Equals(currentHoveredCell))
        {
            Vector3Int previousHoveredCell = currentHoveredCell;
            currentHoveredCell = ClampToBound(cellAtCursor.Value);
            OnHoveredCellChanged(new(previousHoveredCell, currentHoveredCell));
        }
    }

    private Vector3Int ClampToBound(Vector3Int cell)
    {
        int clampedX = Mathf.Clamp(cell.x, 0, CubeGrid.WIDTH - 1);
        int clampedY = Mathf.Clamp(cell.y, 0, CubeGrid.HEIGHT - 1);
        int clampedZ = Mathf.Clamp(cell.z, 0, CubeGrid.DEPTH - 1);
        return new Vector3Int(clampedX, clampedY, clampedZ);
    }

    private Vector3Int? GetCellAtCursor(int y)
    {
        Plane plane = new Plane(Vector3.up, Vector3.up * (y - 0.5f));
        Ray ray = editCameraComponent.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 contactPoint = ray.GetPoint(enter);
            return new Vector3Int(Mathf.RoundToInt(contactPoint.x), y, Mathf.RoundToInt(contactPoint.z));
        }
        return null;
    }

    private void LeftClickSignalMode()
    {
        if (currentSignalTarget != null)
        {
            if (currentSignalTargetType == SignalTargetType.PRODUCER && cubeGridInstanceCreator.IsElementSignalConsumer(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
            {
                if (cubeGrid.ProducerContainsConsumer(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z, currentSignalTarget.x, currentSignalTarget.y, currentSignalTarget.z))
                {
                    cubeGrid.RemoveConsumerFromProducer(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z, currentSignalTarget.x, currentSignalTarget.y, currentSignalTarget.z);
                }
                else
                {
                    cubeGrid.AddConsumerToProducer(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z, currentSignalTarget.x, currentSignalTarget.y, currentSignalTarget.z);
                }
            }
            else if (currentSignalTargetType == SignalTargetType.CONSUMER && cubeGridInstanceCreator.IsElementSignalProducer(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
            {
                if (cubeGrid.ProducerContainsConsumer(currentSignalTarget.x, currentSignalTarget.y, currentSignalTarget.z, currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
                {
                    cubeGrid.RemoveConsumerFromProducer(currentSignalTarget.x, currentSignalTarget.y, currentSignalTarget.z, currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z);
                }
                else
                {
                    cubeGrid.AddConsumerToProducer(currentSignalTarget.x, currentSignalTarget.y, currentSignalTarget.z, currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z);
                }
            }
        }
    }

    private void LeftClickDownEditMode()
    {
        if (!cubeGrid.IsElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
        {
            currentSelectedCell = currentHoveredCell;
            if (cubeGridInstanceCreator.IsElementSignalProducer(currentSelectedCell.x, currentSelectedCell.y, currentSelectedCell.z)
                || cubeGridInstanceCreator.IsElementSignalConsumer(currentSelectedCell.x, currentSelectedCell.y, currentSelectedCell.z))
            {
                StartSignalEditModeForSelectedElement();
            }
            else
            {
                currentEditMode = EditMode.SELECT;
                OnSelectEditModeStarted(new(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z));
            }
        }
        else
        {
            cubeGrid.ChangeElement(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z, currentPrefabIndex, currentRotation);
        }
    }

    private void LeftClickHeldEditMode()
    {
        if (cubeGrid.IsElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
        {
            cubeGrid.ChangeElement(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z, currentPrefabIndex, currentRotation);
        }
    }

    private void RightClickEditMode()
    {
        if (!cubeGrid.IsElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
        {
            cubeGrid.SetElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z);
        }
    }

    private void RotateFreeMode()
    {
        currentRotation = currentRotation.Rotate();
        OnCurrentRotationChanged(EventArgs.Empty);
    }

    private void RotateSelectMode()
    {
        cubeGrid.RotateElement(currentSelectedCell.x, currentSelectedCell.y, currentSelectedCell.z);
    }

    private void LayerChangedHandler(object sender, EditLayerManager.LayerChangedEventArgs layerChangedEventArgs)
    {
        ComputeHoveredCell(layerChangedEventArgs.newHeight);
    }

    private void ElementChangedHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        bool elementWasChanged = elementEventArgs.newElement.prefabIndex != elementEventArgs.previousElement.prefabIndex;
        bool elementIsCurrentSelectFocus = currentSelectedCell.x == elementEventArgs.x && currentSelectedCell.y == elementEventArgs.y && currentSelectedCell.z == elementEventArgs.z;
        bool elementIsCurrentSignalFocus = currentSignalTarget.x == elementEventArgs.x && currentSignalTarget.y == elementEventArgs.y && currentSignalTarget.z == elementEventArgs.z;
        if (elementWasChanged && (currentEditMode == EditMode.SELECT && elementIsCurrentSelectFocus || currentEditMode == EditMode.SIGNAL && elementIsCurrentSignalFocus))
        {
            SetFreeEditMode();
        }
    }

    protected virtual void OnHoveredCellChanged(HoveredCellChangedEventArgs e)
    {
        HoveredCellChanged?.Invoke(this, e);
    }

    protected virtual void OnEditModeStarted(EventArgs e)
    {
        EditModeStarted?.Invoke(this, e);
    }

    protected virtual void OnEditModeStopped(EventArgs e)
    {
        EditModeStopped?.Invoke(this, e);
    }

    protected virtual void OnFreeEditModeStarted(EventArgs e)
    {
        FreeEditModeStarted?.Invoke(this, e);
    }

    protected virtual void OnSelectEditModeStarted(ElementEventArgs e)
    {
        SelectEditModeStarted?.Invoke(this, e);
    }

    protected virtual void OnSignalEditModeStarted(SignalEditModeStartedEventArgs e)
    {
        SignalEditModeStarted?.Invoke(this, e);
    }

    protected virtual void OnCurrentRotationChanged(EventArgs e)
    {
        CurrentRotationChanged?.Invoke(this, e);
    }

    protected virtual void OnCurrentPrefabIndexChanged(EventArgs e)
    {
        CurrentPrefabIndexChanged?.Invoke(this, e);
    }

    public enum SignalTargetType
    {
        PRODUCER,
        CONSUMER
    }

    public enum EditMode
    {
        SELECT,
        SIGNAL,
        FREE
    }

    public class HoveredCellChangedEventArgs : EventArgs
    {
        public Vector3Int previousHoveredCell, newHoveredCell;

        public HoveredCellChangedEventArgs(Vector3Int previousHoveredCell, Vector3Int newHoveredCell)
        {
            this.previousHoveredCell = previousHoveredCell;
            this.newHoveredCell = newHoveredCell;
        }
    }

    public class ElementEventArgs : EventArgs
    {
        public int x, y, z;

        public ElementEventArgs(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class SignalEditModeStartedEventArgs : ElementEventArgs
    {
        public SignalTargetType signalTargetType;

        public SignalEditModeStartedEventArgs(int x, int y, int z, SignalTargetType signalTargetType) : base(x, y, z)
        {
            this.signalTargetType = signalTargetType;
        }
    }
}
