using System;
using UnityEngine;

public class CubeGridPhantomManager : MonoBehaviour
{
    [SerializeField]
    private Material phantomMaterial;
    private CubeGridInstanceManager cubeGridInstanceCreator;
    private CubeGridEditor cubeGridEditor;

    private GameObject phantomCube;

    private void Awake()
    {
        cubeGridInstanceCreator = FindObjectOfType<CubeGridInstanceManager>();
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
    }

    private void Start()
    {
        CubeGridEditor.HoveredCellChanged += HoveredCellChangedHandler;
        CubeGridEditor.CurrentRotationChanged += CurrentRotationChangedHandler;
        CubeGridEditor.CurrentPrefabIndexChanged += CurrentPrefabIndexChangedHandler;
        PlayManager.StartPlaying += StartPlayingHandler;
        CubeGrid.ElementReplaced += ElementReplacedHandler;
    }

    private void HoveredCellChangedHandler(object sender, CubeGridEditor.HoveredCellChangedEventArgs hoveredCellChangedEventArgs)
    {
        if(cubeGridInstanceCreator.GetInstance(hoveredCellChangedEventArgs.newHoveredCell.x, hoveredCellChangedEventArgs.newHoveredCell.y, hoveredCellChangedEventArgs.newHoveredCell.z) == null
             && cubeGridEditor.currentEditMode == CubeGridEditor.EditMode.FREE)
        {
            CreatePhantom(hoveredCellChangedEventArgs.newHoveredCell.x, hoveredCellChangedEventArgs.newHoveredCell.y, hoveredCellChangedEventArgs.newHoveredCell.z);
        }
        else
        {
            DestroyPhantom();
        }
    }

    private void CurrentRotationChangedHandler(object sender, EventArgs eventArgs)
    {
        CreatePhantom(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z);
    }

    private void CurrentPrefabIndexChangedHandler(object sender, EventArgs eventArgs)
    {
        CreatePhantom(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z);
    }

    private void ElementReplacedHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        DestroyPhantom();
    }

    private void StartPlayingHandler(object sender, EventArgs eventArgs)
    {
        DestroyPhantom();
    }

    private void CreatePhantom(int x, int y, int z)
    {
        DestroyPhantom();
        phantomCube = Instantiate(PrefabHelper.PrefabFromIndex(cubeGridEditor.currentPrefabIndex), new Vector3(x, y, z), cubeGridEditor.currentRotation.ToWorldRot());
        phantomCube.GetComponentInChildren<Renderer>().material = phantomMaterial;
    }

    private void DestroyPhantom()
    {
        if (phantomCube != null)
        {
            Destroy(phantomCube);
        }
    }
}
