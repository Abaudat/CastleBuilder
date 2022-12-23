using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public Material selectMaterial;
    public int height = 5, width = 10, depth = 10;
    CubeGridElement[,,] elementGrid;
    GameObject[,,] instancesGrid;

    private Material previousMaterial;

    private void Awake()
    {
        instancesGrid = new GameObject[width, height, depth];
        elementGrid = new CubeGridElement[width, height, depth];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    SetElement(i, j, k, new(Rotation.NORTH, 0));
                }
            }
        }
    }

    public void SetElement(int x, int y, int z, CubeGridElement element)
    {
        elementGrid[x, y, z] = element;
        RecreateElement(x, y, z, element);
    }

    public void RotateElement(int x, int y, int z)
    {
        CubeGridElement element = elementGrid[x, y, z].Rotate();
        elementGrid[x, y, z] = element;
        RecreateElement(x, y, z, element);
    }

    public void ChangeElement(int x, int y, int z, int prefabIndex, Rotation rotation)
    {
        CubeGridElement element = elementGrid[x, y, z].ChangePrefabAndRotation(prefabIndex, rotation);
        elementGrid[x, y, z] = element;
        RecreateElement(x, y, z, element);
    }

    public bool IsElementEmpty(int x, int y, int z)
    {
        return elementGrid[x, y, z].isEmpty();
    }

    public void SelectElement(int x, int y, int z)
    {
        if(elementGrid[x, y, z].isEmpty())
        {
            Debug.LogWarning("Element at " + x + " " + y + " " + z + " is empty, cannot select it");
            return;
        }
        previousMaterial = instancesGrid[x, y, z].GetComponent<Renderer>().material;
        instancesGrid[x, y, z].GetComponent<Renderer>().material = selectMaterial;
    }

    public void UnselectElement(int x, int y, int z)
    {
        if (elementGrid[x, y, z].isEmpty())
        {
            Debug.LogWarning("Element at " + x + " " + y + " " + z + " is empty, cannot unselect it");
            return;
        }
        instancesGrid[x, y, z].GetComponent<Renderer>().material = previousMaterial;
    }

    public void MakeLayersAboveInvisible(int lastVisibleY) //TODO: Improve this to make layer right above transparent, and layers below obvious
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = lastVisibleY + 1; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    GameObject gameObject = instancesGrid[i, j, k];
                    if (gameObject != null)
                    {
                        gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
            }
        }
    }

    public void MakeLayerVisible(int y)
    {
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < depth; k++)
            {
                GameObject gameObject = instancesGrid[i, y, k];
                if (gameObject != null)
                {
                    gameObject.GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }

    public void MakeAllLayersVisible()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    GameObject gameObject = instancesGrid[i, j, k];
                    if (gameObject != null)
                    {
                        gameObject.GetComponent<Renderer>().enabled = true;
                    }
                }
            }
        }
    }

    public void Save(BinaryWriter writer)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    elementGrid[i, j, k].Save(writer);
                }
            }
        }
    }

    public void Load(BinaryReader reader)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    elementGrid[i, j, k].Load(reader);
                    RecreateElement(i, j, k, elementGrid[i, j, k]);
                }
            }
        }
    }

    private void RecreateElement(int x, int y, int z, CubeGridElement element)
    {
        if (instancesGrid[x, y, z] != null)
        {
            Destroy(instancesGrid[x, y, z]);
        }
        if (element.GetPrefab() != null)
        {
            instancesGrid[x, y, z] = Instantiate(element.GetPrefab(), new Vector3(x, y, z), element.rotation.ToWorldRot());
        }
    }

    public class CubeGridElement
    {
        public Rotation rotation;
        public int prefabIndex;

        public CubeGridElement(Rotation rotation, int prefabIndex)
        {
            this.rotation = rotation;
            this.prefabIndex = prefabIndex;
        }

        public CubeGridElement Rotate()
        {
            return new(rotation.Rotate(), prefabIndex);
        }

        public CubeGridElement ChangePrefabAndRotation(int prefabIndex, Rotation rotation)
        {
            return new(rotation, prefabIndex);
        }

        public GameObject GetPrefab()
        {
            return PrefabHelper.PrefabFromIndex(prefabIndex);
        }

        public bool isEmpty()
        {
            return prefabIndex == 0;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)rotation);
            writer.Write(prefabIndex);
        }

        public void Load(BinaryReader reader)
        {
            rotation = (Rotation)reader.ReadByte();
            prefabIndex = reader.ReadInt32();
        }
    }
}
