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
        CubeGridEditor.HoveredCellChanged += ChangeHandler;
        CubeGridEditor.CurrentRotationChanged += ChangeHandler;
        CubeGridEditor.CurrentPrefabIndexChanged += ChangeHandler;
        CubeGridEditor.FreeEditModeStarted += ChangeHandler;
        PlayManager.StartPlaying += StartPlayingHandler;
        CubeGrid.ElementReplaced += ElementReplacedHandler;
    }

    private void ChangeHandler(object sender, EventArgs eventArgs)
    {
        if (cubeGridInstanceCreator.GetInstance(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z) == null
             && cubeGridEditor.currentEditMode == CubeGridEditor.EditMode.BLOCK)
        {
            CreatePhantom(cubeGridEditor.currentHoveredCell.x, cubeGridEditor.currentHoveredCell.y, cubeGridEditor.currentHoveredCell.z);
        }
        else
        {
            DestroyPhantom();
        }
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
