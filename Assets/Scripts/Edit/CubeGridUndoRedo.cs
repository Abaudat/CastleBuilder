using System.Collections.Generic;
using UnityEngine;

public class CubeGridUndoRedo : MonoBehaviour
{
    private CubeGrid cubeGrid;
    private Stack<CubeGridAction> pastActions = new Stack<CubeGridAction>();
    private Stack<CubeGridAction> futureActions = new Stack<CubeGridAction>();

    private bool ignoreNextAction = false;

    private void Awake()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();
    }

    private void Start()
    {
        CubeGrid.ElementReplaced += ElementChangedHandler;
        CubeGrid.ElementRotated += ElementChangedHandler;
        CubeGrid.ElementConsumerAdded += ElementConsumerAddedHandler;
        CubeGrid.ElementConsumerRemoved += ElementConsumerRemovedHandler;
        CubeGrid.GridCleared += GridClearedHandler;
    }

    public void Undo()
    {
        if (pastActions.Count > 0)
        {
            ignoreNextAction = true;
            CubeGridAction lastAction = pastActions.Pop();
            lastAction.Undo(cubeGrid);
            futureActions.Push(lastAction);
        }
    }

    public void Redo()
    {
        if (futureActions.Count > 0)
        {
            ignoreNextAction = true;
            CubeGridAction lastAction = futureActions.Pop();
            lastAction.Redo(cubeGrid);
            pastActions.Push(lastAction);
        }
    }

    private void ElementChangedHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        RegisterAction(new CubeGridReplaceAction(elementEventArgs.x, elementEventArgs.y, elementEventArgs.z, 
            elementEventArgs.previousElement.rotation, elementEventArgs.previousElement.prefabIndex, elementEventArgs.newElement.rotation, elementEventArgs.newElement.prefabIndex));
    }

    private void ElementConsumerAddedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs eventArgs)
    {
        RegisterAction(new CubeGridConsumerAddedAction(eventArgs.x, eventArgs.y, eventArgs.z, eventArgs.consumerCoords));
    }

    private void ElementConsumerRemovedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs eventArgs)
    {
        RegisterAction(new CubeGridConsumerRemovedAction(eventArgs.x, eventArgs.y, eventArgs.z, eventArgs.consumerCoords));
    }

    private void GridClearedHandler(object sender, CubeGrid.GridClearedEventArgs gridClearedEventArgs)
    {
        RegisterAction(new CubeGridClearAllAction(gridClearedEventArgs.previousElementGrid, gridClearedEventArgs.newElementGrid));
    }

    private void RegisterAction(CubeGridAction action)
    {
        if (ignoreNextAction)
        {
            ignoreNextAction = false;
            return;
        }
        pastActions.Push(action);
        futureActions.Clear();
    }

    public abstract class CubeGridAction
    {
        public abstract void Undo(CubeGrid cubeGrid);
        public abstract void Redo(CubeGrid cubeGrid);
    }

    public class CubeGridReplaceAction : CubeGridAction
    {
        private int x, y, z;
        private Rotation previousRot, newRot;
        private int previousPrefabIndex, newPrefabIndex;

        public CubeGridReplaceAction(int x, int y, int z, Rotation previousRot, int previousPrefabIndex, Rotation newRot, int newPrefabIndex)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.previousRot = previousRot;
            this.newRot = newRot;
            this.previousPrefabIndex = previousPrefabIndex;
            this.newPrefabIndex = newPrefabIndex;
        }

        public override void Redo(CubeGrid cubeGrid)
        {
            cubeGrid.ChangeElement(x, y, z, newPrefabIndex, newRot);
        }

        public override void Undo(CubeGrid cubeGrid)
        {
            cubeGrid.ChangeElement(x, y, z, previousPrefabIndex, previousRot);
        }
    }

    public class CubeGridConsumerAddedAction : CubeGridAction
    {
        private int x, y, z;
        private Vector3Int consumerCoords;

        public CubeGridConsumerAddedAction(int x, int y, int z, Vector3Int consumerCoords)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.consumerCoords = consumerCoords;
        }

        public override void Redo(CubeGrid cubeGrid)
        {
            cubeGrid.AddConsumerToProducer(consumerCoords.x, consumerCoords.y, consumerCoords.z, x, y, z);
        }

        public override void Undo(CubeGrid cubeGrid)
        {
            cubeGrid.RemoveConsumerFromProducer(consumerCoords.x, consumerCoords.y, consumerCoords.z, x, y, z);
        }
    }

    public class CubeGridConsumerRemovedAction : CubeGridAction
    {
        private int x, y, z;
        private Vector3Int consumerCoords;

        public CubeGridConsumerRemovedAction(int x, int y, int z, Vector3Int consumerCoords)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.consumerCoords = consumerCoords;
        }

        public override void Redo(CubeGrid cubeGrid)
        {
            cubeGrid.RemoveConsumerFromProducer(consumerCoords.x, consumerCoords.y, consumerCoords.z, x, y, z);
        }

        public override void Undo(CubeGrid cubeGrid)
        {
            cubeGrid.AddConsumerToProducer(consumerCoords.x, consumerCoords.y, consumerCoords.z, x, y, z);
        }
    }

    public class CubeGridClearAllAction : CubeGridAction
    {
        private CubeGrid.CubeGridElement[,,] previousGrid, newGrid;

        public CubeGridClearAllAction(CubeGrid.CubeGridElement[,,] previousGrid, CubeGrid.CubeGridElement[,,] newGrid)
        {
            this.previousGrid = previousGrid;
            this.newGrid = newGrid;
        }

        public override void Redo(CubeGrid cubeGrid)
        {
            cubeGrid.SetElementGrid(newGrid);
        }

        public override void Undo(CubeGrid cubeGrid)
        {
            cubeGrid.SetElementGrid(previousGrid);
        }
    }
}
