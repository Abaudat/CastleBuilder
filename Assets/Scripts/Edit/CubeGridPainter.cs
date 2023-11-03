using System;
using System.Linq;
using UnityEngine;

public class CubeGridPainter : MonoBehaviour
{
    private CubeGridInstanceManager cubeGridInstanceCreator;
    private CubeGrid cubeGrid;
    private CubeGridEditor cubeGridEditor;
    private EditLayerManager editLayerManager;

    private void Awake()
    {
        cubeGridInstanceCreator = FindObjectOfType<CubeGridInstanceManager>();
        cubeGrid = FindObjectOfType<CubeGrid>();
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        editLayerManager = FindObjectOfType<EditLayerManager>();
    }

    private void Start()
    {
        EditLayerManager.LayerChanged += LayerChangedHandler;
        CubeGridEditor.FreeEditModeStarted += FreeEditModeStartedHandler;
        CubeGridEditor.SelectEditModeStarted += SelectEditModeStartedHandler;
        CubeGridEditor.SignalEditModeStarted += SignalEditModeStartedHandler;
        CubeGridEditor.HoveredCellChanged += HoveredCellChangedHandler;
        PlayManager.StartPlaying += StartPlayingHandler;
        CubeGrid.ElementReplaced += ElementChangedHandler;
        CubeGrid.ElementRotated += ElementChangedHandler;
        CubeGrid.ElementConsumerAdded += ElementConsumerModifiedHandler;
        CubeGrid.ElementConsumerRemoved += ElementConsumerModifiedHandler;
    }

    private void LayerChangedHandler(object sender, EditLayerManager.LayerChangedEventArgs layerChangedEventArgs)
    {
        Repaint();
    }

    private void FreeEditModeStartedHandler(object sender, EventArgs eventArgs)
    {
        PaintForFreeEditMode();
    }

    private void SelectEditModeStartedHandler(object sender, CubeGridEditor.ElementEventArgs elementEventArgs)
    {
        PaintForSelectEditMode(elementEventArgs.x, elementEventArgs.y, elementEventArgs.z);
    }

    private void SignalEditModeStartedHandler(object sender, CubeGridEditor.SignalEditModeStartedEventArgs signalEditModeStartedEventArgs)
    {
        PaintForSignalEditMode(signalEditModeStartedEventArgs.x, signalEditModeStartedEventArgs.y, signalEditModeStartedEventArgs.z, signalEditModeStartedEventArgs.signalTargetType);
    }

    private void StartPlayingHandler(object sender, EventArgs eventArgs)
    {
        ChangeAllMaterials(x => x.Reset());
    }

    private void HoveredCellChangedHandler(object sender, CubeGridEditor.HoveredCellChangedEventArgs hoveredCellChangedEventArgs)
    {
        Repaint();
    }

    private void ElementConsumerModifiedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs elementConsumerModifiedEventArgs)
    {
        Repaint();
    }

    private void ElementChangedHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        Repaint();
    }

    private void Repaint()
    {
        switch (cubeGridEditor.currentEditMode)
        {
            case CubeGridEditor.EditMode.SELECT:
                PaintForSelectEditMode(cubeGridEditor.currentSelectedCell.x, cubeGridEditor.currentSelectedCell.y, cubeGridEditor.currentSelectedCell.z);
                break;
            case CubeGridEditor.EditMode.SIGNAL:
                PaintForSignalEditMode(cubeGridEditor.currentSignalTarget.x, cubeGridEditor.currentSignalTarget.y, cubeGridEditor.currentSignalTarget.z, cubeGridEditor.currentSignalTargetType);
                break;
            case CubeGridEditor.EditMode.FREE:
            case CubeGridEditor.EditMode.BLOCK:
                PaintForFreeEditMode();
                break;
        }
        HighlightHoveredCell();
    }

    private void HighlightHoveredCell()
    {
        if (cubeGridInstanceCreator.GetInstance(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z) != null)
        {
            bool isBlockEditMode = cubeGridEditor.currentEditMode == CubeGridEditor.EditMode.BLOCK;
            bool isFreeEditMode = cubeGridEditor.currentEditMode == CubeGridEditor.EditMode.FREE;
            bool isSignalEditMode = cubeGridEditor.currentEditMode == CubeGridEditor.EditMode.SIGNAL;
            bool isHoveredCellOfOppositeTypeAndUnlinked = (cubeGridEditor.currentSignalTargetType == CubeGridEditor.SignalTargetType.PRODUCER
                    && cubeGridInstanceCreator.IsElementSignalConsumer(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z)
                    && !cubeGrid.ConsumersForProducer(cubeGridEditor.currentSignalTarget.x, cubeGridEditor.currentSignalTarget.y, cubeGridEditor.currentSignalTarget.z).Contains(cubeGridEditor.currentHoveredCell))
                    || (cubeGridEditor.currentSignalTargetType == CubeGridEditor.SignalTargetType.CONSUMER
                    && cubeGridInstanceCreator.IsElementSignalProducer(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z)
                    && !cubeGrid.ProducersForConsumer(cubeGridEditor.currentSignalTarget.x, cubeGridEditor.currentSignalTarget.y, cubeGridEditor.currentSignalTarget.z).Contains(cubeGridEditor.currentHoveredCell));
            if (isFreeEditMode || isBlockEditMode || (isSignalEditMode && isHoveredCellOfOppositeTypeAndUnlinked))
            {
                ChangeMaterial(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z, x => x.Hover()); //TODO: Hover in red if about to unlink
            }
        }
    }

    private void PaintForFreeEditMode()
    {
        int currentLayerY = editLayerManager.currentHeight;
        ChangeMaterialsOfLayersBelow(x => x.Shadow(), currentLayerY);
        ChangeMaterialsOfLayer(x => x.Reset(), currentLayerY);
        ChangeMaterialsOfLayer(x => x.Transparent(), currentLayerY + 1);
        ChangeMaterialsOfLayersAbove(x => x.Invisible(), currentLayerY + 1);
    }

    private void PaintForSelectEditMode(int selectedElementX, int selectedElementY, int selectedElementZ)
    {
        PaintForFreeEditMode();
        ChangeMaterial(selectedElementX, selectedElementY, selectedElementZ, x => x.Select());
    }

    private void PaintForSignalEditMode(int selectedElementX, int selectedElementY, int selectedElementZ, CubeGridEditor.SignalTargetType signalTargetType)
    {
        int currentLayerY = editLayerManager.currentHeight;
        ChangeMaterialsOfLayersBelow(x => x.Shadow(), currentLayerY + 1);
        ChangeMaterialsOfLayer(x => x.Transparent(), currentLayerY + 1);
        ChangeMaterialsOfLayersAbove(x => x.Invisible(), currentLayerY + 1);
        if (signalTargetType == CubeGridEditor.SignalTargetType.PRODUCER)
        {
            ChangeAllConsumersUnderMaterials(currentLayerY, k => k.HighlightUnlinkedUnder());
            ChangeAllConsumersSameLayerMaterials(currentLayerY, k => k.HighlightUnlinked());
            ChangeAllLinkedConsumersUnderMaterials(currentLayerY, selectedElementX, selectedElementY, selectedElementZ, k => k.HighlightLinkedUnder());
            ChangeAllLinkedConsumersSameLayerMaterials(currentLayerY, selectedElementX, selectedElementY, selectedElementZ, k => k.HighlightLinked());
        }
        else if (signalTargetType == CubeGridEditor.SignalTargetType.CONSUMER)
        {
            ChangeAllProducersUnderMaterials(currentLayerY, k => k.HighlightUnlinkedUnder());
            ChangeAllProducersSameLayerMaterials(currentLayerY, k => k.HighlightUnlinked());
            ChangeAllLinkedProducersUnderMaterials(currentLayerY, selectedElementX, selectedElementY, selectedElementZ, k => k.HighlightLinkedUnder());
            ChangeAllLinkedProducersSameLayerMaterials(currentLayerY, selectedElementX, selectedElementY, selectedElementZ, k => k.HighlightLinked());
        }
        ChangeMaterial(selectedElementX, selectedElementY, selectedElementZ, k => k.Select());
    }

    private void ChangeMaterial(int x, int y, int z, Action<DisplayManager> action)
    {
        if (cubeGridInstanceCreator.GetInstance(x, y, z) != null)
        {
            foreach (DisplayManager displayManager in cubeGridInstanceCreator.GetInstance(x, y, z).GetComponentsInChildren<DisplayManager>())
            {
                action.Invoke(displayManager);
            }
        }
    }

    private void ChangeAllMaterials(Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (_, _, _) => true);
    }

    private void ChangeAllMaterials(Action<DisplayManager> action, Func<int, int, int, bool> filter)
    {
        for (int i = 0; i < CubeGrid.WIDTH; i++)
        {
            for (int j = 0; j < CubeGrid.HEIGHT; j++)
            {
                for (int k = 0; k < CubeGrid.DEPTH; k++)
                {
                    if (filter.Invoke(i, j, k))
                    {
                        ChangeMaterial(i, j, k, action);
                    }
                }
            }
        }
    }

    private void ChangeMaterialsOfLayer(Action<DisplayManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y == layerY);
    }

    private void ChangeMaterialsOfLayersBelow(Action<DisplayManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y < layerY);
    }

    private void ChangeMaterialsOfLayersAbove(Action<DisplayManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y > layerY);
    }

    private void ChangeAllConsumersUnderMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && cubeGridInstanceCreator.IsElementSignalConsumer(x, y, z));
    }

    private void ChangeAllProducersUnderMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && cubeGridInstanceCreator.IsElementSignalProducer(x, y, z));
    }

    private void ChangeAllConsumersSameLayerMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && cubeGridInstanceCreator.IsElementSignalConsumer(x, y, z));
    }

    private void ChangeAllProducersSameLayerMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && cubeGridInstanceCreator.IsElementSignalProducer(x, y, z));
    }

    private void ChangeAllLinkedConsumersUnderMaterials(int layerY, int producerX, int producerY, int producerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && cubeGrid.ConsumersForProducer(producerX, producerY, producerZ).Any(consumerCoords => consumerCoords == new Vector3Int(x, y, z)));
    }

    private void ChangeAllLinkedConsumersSameLayerMaterials(int layerY, int producerX, int producerY, int producerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && cubeGrid.ConsumersForProducer(producerX, producerY, producerZ).Any(consumerCoords => consumerCoords == new Vector3Int(x, y, z)));
    }

    private void ChangeAllLinkedProducersUnderMaterials(int layerY, int consumerX, int consumerY, int consumerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && cubeGrid.ConsumersForProducer(x, y, z).Any(consumerCoords => consumerCoords == new Vector3Int(consumerX, consumerY, consumerZ)));
    }

    private void ChangeAllLinkedProducersSameLayerMaterials(int layerY, int consumerX, int consumerY, int consumerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && cubeGrid.ConsumersForProducer(x, y, z).Any(consumerCoords => consumerCoords == new Vector3Int(consumerX, consumerY, consumerZ)));
    }
}
