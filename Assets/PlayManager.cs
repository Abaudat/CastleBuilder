using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Transform playerSpawnPosition;
    public GameObject playerPrefab, winPanel, losePanel;

    private CubeGridEditor cubeGridEditor;

    bool isExploring = false;
    bool isPlaying = false;
    GameObject exploringPlayer;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
    }

    private void Update()
    {
        if (isExploring)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StopExploring();
            }
        }
    }

    public void StartExploring(int x, int y, int z)
    {
        isExploring = true;
        exploringPlayer = Instantiate(playerPrefab, new Vector3(x, y, z), Quaternion.identity);
    }

    public void StartPlaying()
    {
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
