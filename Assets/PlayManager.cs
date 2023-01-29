using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Transform playerSpawnPosition;
    public GameObject playerPrefab, winPanel, losePanel, editCamera;

    private CubeGridEditor cubeGridEditor;
    private CubeGrid cubeGrid;
    private TutorialManager tutorialManager;

    bool isExploring = false;
    bool isValidating = false;
    GameObject exploringPlayer;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        cubeGrid = FindObjectOfType<CubeGrid>();
        tutorialManager = FindObjectOfType<TutorialManager>();
    }

    private void Update()
    {
        if (isExploring)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopExploring();
            }
        }
        if (isValidating)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopPlaying();
            }
        }
    }

    public void StartExploring(int x, int y, int z)
    {
        if (!isExploring)
        {
            editCamera.SetActive(false);
            cubeGrid.PrepareForPlay();
            isExploring = true;
            exploringPlayer = Instantiate(playerPrefab, new Vector3(x, y, z), Quaternion.identity);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void StartValidating()
    {
        if (!isValidating)
        {
            tutorialManager.TogglePlayModeTutorial();
            editCamera.SetActive(false);
            cubeGrid.PrepareForPlay();
            if (exploringPlayer != null)
            {
                Destroy(exploringPlayer);
            }
            isValidating = true;
            exploringPlayer = Instantiate(playerPrefab, playerSpawnPosition.position, playerSpawnPosition.rotation);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void StopExploring()
    {
        isExploring = false;
        Destroy(exploringPlayer);
        cubeGridEditor.StartEditing();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StopPlaying()
    {
        isValidating = false;
        Destroy(exploringPlayer);
        cubeGridEditor.StartEditing();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Success()
    {
        if (isValidating)
        {
            isValidating = false;
            exploringPlayer.GetComponent<PlayerMover>().canMove = false;
            winPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Die()
    {
        if (isValidating || isExploring)
        {
            isValidating = isExploring = false;
            exploringPlayer.GetComponent<PlayerMover>().canMove = false;
            losePanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
