using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGridEditor : MonoBehaviour
{
    public GameObject editCamera;
    public CubeGrid cubeGrid;
    public GameObject phantomPrefab, playerPrefab;

    private int currentX, currentY, currentZ;
    private GameObject phantomCube, exploringPlayer;
    private bool isEditing, isExploring;

    private void Awake()
    {
        StartEditing();
    }

    private void Update()
    {
        if (isEditing)
        {
            //TODO: Use layout-insensitive key mappings
            if (Input.GetKeyDown(KeyCode.W))
            {
                ChangeCurrentCell(currentX, currentY, currentZ + 1);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeCurrentCell(currentX + 1, currentY, currentZ);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeCurrentCell(currentX, currentY, currentZ - 1);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeCurrentCell(currentX - 1, currentY, currentZ);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeCurrentCell(currentX, currentY + 1, currentZ);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeCurrentCell(currentX, currentY - 1, currentZ);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeCurrentPrefab(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeCurrentPrefab(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeCurrentPrefab(2);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RotateCurrent();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopEditing();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                StartExploring();
            }
        }
        else if (isExploring)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StopExploring();
            }
        }
    }

    public void StartExploring()
    {
        isExploring = true;
        exploringPlayer = Instantiate(playerPrefab, new Vector3(currentX, currentY, currentZ), Quaternion.identity);
        StopEditing();
    }

    public void StopExploring()
    {
        isExploring = false;
        Destroy(exploringPlayer);
        StartEditing();
    }

    public void StartEditing()
    {
        isEditing = true;
        editCamera.SetActive(true);
        phantomCube = Instantiate(phantomPrefab);
    }

    public void StopEditing()
    {
        isEditing = false;
        editCamera.SetActive(false);
        Destroy(phantomCube);
    }

    public void ChangeCurrentCell(int x, int y, int z)
    {
        if(x >= 0 && x < cubeGrid.width && y >= 0 && y < cubeGrid.height && z >= 0 && z < cubeGrid.depth)
        {
            currentX = x;
            currentY = y;
            currentZ = z;
            phantomCube.transform.position = new Vector3(x, y, z);
        }
    }

    public void RotateCurrent()
    {
        cubeGrid.RotateElement(currentX, currentY, currentZ);
    }

    public void ChangeCurrentPrefab(int prefabIndex)
    {
        cubeGrid.ChangeElementPrefab(currentX, currentY, currentZ, prefabIndex);
    }
}
