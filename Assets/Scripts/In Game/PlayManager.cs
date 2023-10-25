using System;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static event EventHandler StartPlaying;

    public Transform playerSpawnPosition;
    public GameObject playerPrefab, validateWinPanel, validateLosePanel, playWinPanel, playLosePanel, noChestPanel, levelBrowserPanel;

    private CubeGridEditor cubeGridEditor;
    private CubeGrid cubeGrid;
    private CubeGridInstanceManager cubeGridInstanceManager;
    private LevelBrowserManager levelBrowserManager;

    bool isValidating = false;
    bool isPlaying = false;
    GameObject exploringPlayer;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        cubeGrid = FindObjectOfType<CubeGrid>();
        cubeGridInstanceManager = FindObjectOfType<CubeGridInstanceManager>();
        levelBrowserManager = FindObjectOfType<LevelBrowserManager>();
    }

    private void Update()
    {
        if (isValidating && Input.GetKeyDown(KeyCode.Escape))
        {
            StopPlaying();
            cubeGridEditor.StartEditing();
        }
        if (isPlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            StopPlaying();
            levelBrowserManager.Browse();
        }
    }

    public void Validate()
    {
        if (!isValidating && !isPlaying)
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

    public void Play()
    {
        if (!isValidating && !isPlaying)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (exploringPlayer != null)
            {
                Destroy(exploringPlayer);
            }
            isPlaying = true;
            exploringPlayer = Instantiate(playerPrefab, playerSpawnPosition.position, playerSpawnPosition.rotation);
            cubeGridEditor.StopEditing();
            cubeGridInstanceManager.RecreateAllElements();
            cubeGridInstanceManager.TriggerAllSelfdestructs();
            cubeGridInstanceManager.EnableAllRigidbodies();
            OnStartPlaying(EventArgs.Empty);
        }
    }

    public void StopPlaying()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isValidating = false;
        isPlaying = false;
        Destroy(exploringPlayer);
        cubeGridInstanceManager.RecreateAllElements();
    }

    public void Success()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        exploringPlayer.GetComponent<PlayerMover>().canMove = false;
        if (isValidating)
        {
            isValidating = false;
            validateWinPanel.SetActive(true);
        }
        else if (isPlaying)
        {
            isPlaying = false;
            playWinPanel.SetActive(true);
        }
    }

    public void Die()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        exploringPlayer.GetComponent<PlayerMover>().canMove = false;
        if (isValidating)
        {
            isValidating = false;
            validateLosePanel.SetActive(true);
        }
        else if (isPlaying)
        {
            isPlaying = false;
            playLosePanel.SetActive(true);
        }
    }

    protected virtual void OnStartPlaying(EventArgs e)
    {
        StartPlaying?.Invoke(this, e);
    }
}
