using System;
using UnityEngine;

public class EditModeObjectsManager : MonoBehaviour
{
    public GameObject editCamera, editPanel, roof, editLayerPlanePrefab;
    public Renderer rightWall, aboveWall, leftWall, belowWall;
    public Material wallMat;

    private EditLayerManager editLayerManager;

    private GameObject editLayerPlane;

    private void Awake()
    {
        editLayerManager = FindObjectOfType<EditLayerManager>();
    }

    private void Start()
    {
        CubeGridEditor.EditModeStarted += EditModeStartedHandler;
        CubeGridEditor.EditModeStopped += EditModeStoppedHandler;
        PlayManager.StartPlaying += StartPlayingHandler;
        EditLayerManager.LayerChanged += LayerChangedHandler;
    }

    private void EditModeStartedHandler(object sender, EventArgs eventArgs)
    {
        StartEditMode();
    }

    private void EditModeStoppedHandler(object sender, EventArgs eventArgs)
    {
        DeletePlane();
    }

    private void StartPlayingHandler(object sender, EventArgs eventArgs)
    {
        StartPlayMode();
    }

    private void LayerChangedHandler(object sender, EventArgs eventArgs)
    {
        editLayerPlane.transform.position = new Vector3(4.5f, editLayerManager.currentHeight - 0.49f, 4.5f);
    }

    private void StartEditMode()
    {
        editPanel.SetActive(true);
        editCamera.SetActive(true);
        roof.SetActive(false);
        editLayerPlane = Instantiate(editLayerPlanePrefab, new Vector3(4.5f, editLayerManager.currentHeight - 0.49f, 4.5f), Quaternion.identity);
    }

    private void StartPlayMode()
    {
        editPanel.SetActive(false);
        editCamera.SetActive(false);
        roof.SetActive(true);
        belowWall.material = wallMat;
        rightWall.material = wallMat;
        aboveWall.material = wallMat;
        leftWall.material = wallMat;
        DeletePlane();
    }

    private void DeletePlane()
    {
        Destroy(editLayerPlane);
    }
}
