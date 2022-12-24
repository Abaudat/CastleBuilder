using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Transform playerSpawnPosition;
    public GameObject playerPrefab, winPanel, losePanel;

    private CubeGridEditor cubeGridEditor;
    private CubeGrid cubeGrid;

    bool isExploring = false;
    bool isPlaying = false;
    GameObject exploringPlayer;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        cubeGrid = FindObjectOfType<CubeGrid>();
    }

    private void Update()
    {
        if (isExploring)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                StopExploring();
            }
        }
        if (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopPlaying();
            }
        }
    }

    public void StartExploring(int x, int y, int z)
    {
        cubeGrid.PrepareForPlay();
        isExploring = true;
        exploringPlayer = Instantiate(playerPrefab, new Vector3(x, y, z), Quaternion.identity);
    }

    public void StartPlaying()
    {
        cubeGrid.PrepareForPlay();
        if (exploringPlayer != null)
        {
            Destroy(exploringPlayer);
        }
        isPlaying = true;
        exploringPlayer = Instantiate(playerPrefab, playerSpawnPosition.position, playerSpawnPosition.rotation);
    }

    public void StopExploring()
    {
        isExploring = false;
        Destroy(exploringPlayer);
        cubeGridEditor.StartEditing();
    }

    public void StopPlaying()
    {
        isPlaying = false;
        Destroy(exploringPlayer);
        cubeGridEditor.StartEditing();
    }

    public void Success()
    {
        exploringPlayer.GetComponent<PlayerMover>().canMove = false;
        winPanel.SetActive(true);
    }

    public void Die()
    {
        exploringPlayer.GetComponent<PlayerMover>().canMove = false;
        losePanel.SetActive(true);
    }
}
