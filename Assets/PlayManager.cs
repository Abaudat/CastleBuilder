using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Transform playerSpawnPosition;
    public GameObject playerPrefab;

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
        isPlaying = true;
        Instantiate(playerPrefab, playerSpawnPosition.position, Quaternion.identity);
    }

    public void StopExploring()
    {
        isExploring = false;
        Destroy(exploringPlayer);
        cubeGridEditor.StartEditing();
    }
}
