using System;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static event EventHandler StartPlaying;

    public Transform playerSpawnPosition;
    public GameObject playerPrefab, winPanel, losePanel, editCamera, noChestPanel;

    private CubeGridEditor cubeGridEditor;
    private CubeGrid cubeGrid;
    private CubeGridInstanceManager cubeGridInstanceManager;

    bool isValidating = false;
    GameObject exploringPlayer;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        cubeGrid = FindObjectOfType<CubeGrid>();
        cubeGridInstanceManager = FindObjectOfType<CubeGridInstanceManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopPlaying();
        }
    }

    public void StartValidating()
    {
        if (!isValidating)
        {
            if (cubeGrid.ContainsAtLeastOneChest())
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                if (exploringPlayer != null)
                {
                    Destroy(exploringPlayer);
                }
                isValidating = true;
                exploringPlayer = Instantiate(playerPrefab, playerSpawnPosition.position, playerSpawnPosition.rotation);
                cubeGridEditor.StopEditing();
                cubeGridInstanceManager.RecreateAllElements();
                cubeGridInstanceManager.TriggerAllSelfdestructs();
                cubeGridInstanceManager.EnableAllRigidbodies();
                OnStartPlaying(EventArgs.Empty);
            }
            else
            {
                noChestPanel.SetActive(true);
            }
        }
    }

    public void StopPlaying()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isValidating = false;
        Destroy(exploringPlayer);
        cubeGridInstanceManager.RecreateAllElements();
        cubeGridEditor.StartEditing();
    }

    public void Success()
    {
        if (isValidating)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isValidating = false;
            exploringPlayer.GetComponent<PlayerMover>().canMove = false;
            winPanel.SetActive(true);
        }
    }

    public void Die()
    {
        if (isValidating)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isValidating = false;
            exploringPlayer.GetComponent<PlayerMover>().canMove = false;
            losePanel.SetActive(true);
        }
    }

    protected virtual void OnStartPlaying(EventArgs e)
    {
        StartPlaying?.Invoke(this, e);
    }
}
