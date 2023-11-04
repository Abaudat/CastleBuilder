using System;
using UnityEngine;

public class CubeGridInstanceManager : MonoBehaviour
{
    public static event EventHandler<GridInstantiatedEventArgs> GridInstantiated;

    private CubeGrid cubeGrid;

    GameObject[,,] instancesGrid;

    private void Awake()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();
        instancesGrid = new GameObject[CubeGrid.WIDTH, CubeGrid.HEIGHT, CubeGrid.DEPTH];
    }

    private void Start()
    {
        CubeGrid.ElementReplaced += ElementChangeHandler;
        CubeGrid.ElementRotated += ElementChangeHandler; // TODO: Add animation for rotation instead of recreating
        CubeGrid.GridLoaded += GridLoadedHandler;
        CubeGrid.GridCleared += GridClearedHandler;
    }

    public GameObject GetInstance(int x, int y, int z)
    {
        if (x >= 0 && x < CubeGrid.WIDTH && y >= 0 && y < CubeGrid.HEIGHT && z >= 0 && z < CubeGrid.DEPTH)
        {
            return instancesGrid[x, y, z];
        }
        Debug.LogWarning("Out of bounds: " + x + " " + y + " " + z);
        return null;
    }

    public void EnableAllRigidbodies()
    {
        for (int i = 0; i < CubeGrid.WIDTH; i++)
        {
            for (int j = 0; j < CubeGrid.HEIGHT; j++)
            {
                for (int k = 0; k < CubeGrid.DEPTH; k++)
                {
                    GameObject instance = instancesGrid[i, j, k];
                    if (instance != null)
                    {
                        instance.EnableRigidbody();
                    }
                }
            }
        }
    }

    public void TriggerAllSelfdestructs()
    {
        for (int i = 0; i < CubeGrid.WIDTH; i++)
        {
            for (int j = 0; j < CubeGrid.HEIGHT; j++)
            {
                for (int k = 0; k < CubeGrid.DEPTH; k++)
                {
                    GameObject instance = instancesGrid[i, j, k];
                    if (instance != null)
                    {
                        PlayModeSelfdestruct playModeSelfdestruct = instance.TryGetComponentInChildren<PlayModeSelfdestruct>();
                        if (playModeSelfdestruct != null)
                        {
                            playModeSelfdestruct.SelfDestruct();
                        }
                    }
                }
            }
        }
    }

    public bool IsElementSignalProducer(int x, int y, int z)
    {
        GameObject instance = GetInstance(x, y, z);
        if (instance != null)
        {
            return instance.TryGetComponentInChildren<SignalProducer>() != null;
        }
        return false;
    }

    public bool IsElementSignalConsumer(int x, int y, int z)
    {
        GameObject instance = GetInstance(x, y, z);
        if (instance != null)
        {
            return instance.TryGetComponentInChildren<SignalConsumer>() != null;
        }
        return false;
    }

    protected virtual void OnGridInstantiated(GridInstantiatedEventArgs e)
    {
        GridInstantiated?.Invoke(this, e);
    }

    private void ElementChangeHandler(object sender, CubeGrid.ElementEventArgs elementEventArgs)
    {
        RecreateElement(elementEventArgs.x, elementEventArgs.y, elementEventArgs.z, elementEventArgs.newElement);
    }

    private void GridLoadedHandler(object sender, CubeGrid.GridLoadedEventArgs gridLoadedEventArgs)
    {
        RecreateAllElements();
    }

    private void GridClearedHandler(object sender, CubeGrid.GridClearedEventArgs gridClearedEventArgs)
    {
        RecreateAllElements();
    }

    private void RecreateElement(int x, int y, int z, CubeGrid.CubeGridElement element)
    {
        if (instancesGrid[x, y, z] != null)
        {
            Destroy(instancesGrid[x, y, z]);
        }
        if (PrefabHelper.PrefabFromIndex(element.prefabIndex) != null)
        {
            GameObject gameObject = Instantiate(PrefabHelper.PrefabFromIndex(element.prefabIndex), new Vector3(x, y, z), element.rotation.ToWorldRot());
            instancesGrid[x, y, z] = gameObject;
        }
    }

    public void RecreateAllElements()
    {
        for (int i = 0; i < CubeGrid.WIDTH; i++)
        {
            for (int j = 0; j < CubeGrid.HEIGHT; j++)
            {
                for (int k = 0; k < CubeGrid.DEPTH; k++)
                {
                    RecreateElement(i, j, k, cubeGrid.elementGrid[i, j, k]);
                }
            }
        }
        OnGridInstantiated(new(instancesGrid));
    }

    public class GridInstantiatedEventArgs : EventArgs
    {
        public GameObject[,,] instancesGrid;

        public GridInstantiatedEventArgs(GameObject[,,] instancesGrid)
        {
            this.instancesGrid = instancesGrid;
        }
    }
}
