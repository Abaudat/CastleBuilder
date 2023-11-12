using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class CubeGridEditor : MonoBehaviour
{
    public static event EventHandler<HoveredCellChangedEventArgs> HoveredCellChanged;
    public static event EventHandler EditModeStarted;
    public static event EventHandler EditModeStopped;
    public static event EventHandler FreeEditModeStarted;
    public static event EventHandler<ElementEventArgs> SelectEditModeStarted;
    public static event EventHandler<SignalEditModeStartedEventArgs> SignalEditModeStarted;
    public static event EventHandler BlockEditModeStarted;
    public static event EventHandler CurrentRotationChanged;
    public static event EventHandler<PrefabIndexEventArgs> CurrentPrefabIndexChanged;

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
    private bool leftClickHeld = false, rightClickHeld = false;

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
            if (leftClickHeld)
            {
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                    case EditMode.FREE:
                        break;
                    case EditMode.BLOCK:
                        LeftClickHeldEditMode();
                        break;
                }
            }
            if (rightClickHeld)
            {
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                        break;
                    case EditMode.FREE:
                        if (!rightClickExitTriggered)
                        {
                            RightClickFreeEditMode();
                        }
                        break;
                    case EditMode.BLOCK:
                        if (!rightClickExitTriggered)
                        {
                            RightClickBlockEditMode();
                        }
                        break;
                }
            }
        }
    }

    public void OnLeftClickInput(CallbackContext callbackContext)
    {
        if (isEditing && !eventSystem.IsPointerOverGameObject())
        {
            if (callbackContext.started)
            {
                leftClickHeld = true;
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                        LeftClickSignalMode();
                        break;
                    case EditMode.FREE:
                        LeftClickDownFreeEditMode();
                        break;
                    case EditMode.BLOCK:
                        LeftClickDownBlockEditMode();
                        break;
                    case EditMode.SELECT:
                        // TODO: Allow selecting another
                        break;
                }
            }
            else if (callbackContext.canceled)
            {
                leftClickHeld = false;
            }
        }
    }

    public void OnRightClickInput(CallbackContext callbackContext)
    {
        if (isEditing && !eventSystem.IsPointerOverGameObject())
        {
            if (callbackContext.started)
            {
                rightClickHeld = true;
                switch (currentEditMode)
                {
                    case EditMode.SIGNAL:
                    case EditMode.SELECT:
                        soundManager.PlayCancelClip();
                        rightClickExitTriggered = true;
                        SetFreeEditMode();
                        break;
                    case EditMode.FREE:
                        RightClickFreeEditMode();
                        break;
                    case EditMode.BLOCK:
                        RightClickBlockEditMode();
                        break;
                }
            }
            else if (callbackContext.canceled)
            {
                rightClickExitTriggered = false;
                rightClickHeld = false;
            }
        }
    }

    public void OnInteractInput(CallbackContext callbackContext)
    {
        if (isEditing && !eventSystem.IsPointerOverGameObject() && callbackContext.started)
        {
            switch (currentEditMode)
            {
                case EditMode.BLOCK:
                    RotateBlockMode();
                    break;
                case EditMode.SIGNAL:
                case EditMode.SELECT:
                    RotateSelectMode();
                    break;
                case EditMode.FREE:
                    break;
            }
        }
    }

    public void OnEscapeInput(CallbackContext callbackContext)
    {
        if (isEditing && !eventSystem.IsPointerOverGameObject() && callbackContext.started)
        {
            switch (currentEditMode)
            {
                case EditMode.FREE:
                    break;
                case EditMode.SIGNAL:
                case EditMode.SELECT:
                case EditMode.BLOCK:
                    SetFreeEditMode();
                    break;
            }
        }
    }

    public void OnUndoInput(CallbackContext callbackContext)
    {
        if (isEditing && !eventSystem.IsPointerOverGameObject() && callbackContext.started)
        {
            cubeGridUndoRedo.Undo();
        }
    }

    public void OnRedoInput(CallbackContext callbackContext)
    {
        if (isEditing && !eventSystem.IsPointerOverGameObject() && callbackContext.started)
        {
            cubeGridUndoRedo.Redo();
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
        OnCurrentPrefabIndexChanged(new(newPrefabIndex));
        SetBlockEditMode();
    }

    public void SetFreeEditMode()
    {
        currentEditMode = EditMode.FREE;
        OnFreeEditModeStarted(EventArgs.Empty);
    }

    public void SetBlockEditMode()
    {
        currentEditMode = EditMode.BLOCK;
        OnBlockEditModeStarted(EventArgs.Empty);
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
        Ray ray = editCameraComponent.ScreenPointToRay(Mouse.current.position.ReadValue());
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

    private void LeftClickDownBlockEditMode()
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

    private void LeftClickDownFreeEditMode()
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
    }

    private void LeftClickHeldEditMode()
    {
        if (cubeGrid.IsElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
        {
            cubeGrid.ChangeElement(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z, currentPrefabIndex, currentRotation);
        }
    }

    private void RightClickBlockEditMode()
    {
        if (!cubeGrid.IsElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
        {
            cubeGrid.SetElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z);
        }
        else
        {
            soundManager.PlayCancelClip();
            SetFreeEditMode();
        }
    }

    private void RightClickFreeEditMode()
    {
        if (!cubeGrid.IsElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z))
        {
            cubeGrid.SetElementEmpty(currentHoveredCell.x, currentHoveredCell.y, currentHoveredCell.z);
        }
    }

    private void RotateBlockMode()
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
            SetBlockEditMode();
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

    protected virtual void OnBlockEditModeStarted(EventArgs e)
    {
        BlockEditModeStarted?.Invoke(this, e);
    }

    protected virtual void OnCurrentRotationChanged(EventArgs e)
    {
        CurrentRotationChanged?.Invoke(this, e);
    }

    protected virtual void OnCurrentPrefabIndexChanged(PrefabIndexEventArgs e)
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
        FREE,
        BLOCK
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

    public class PrefabIndexEventArgs : EventArgs
    {
        public int prefabIndex;

        public PrefabIndexEventArgs(int prefabIndex)
        {
            this.prefabIndex = prefabIndex;
        }
    }
}
