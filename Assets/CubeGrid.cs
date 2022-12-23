using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public int height = 5, width = 10, depth = 10;
    CubeGridElement[,,] elementGrid;
    GameObject[,,] instancesGrid;

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

    public void ChangeElementPrefab(int x, int y, int z, int prefabIndex)
    {
        CubeGridElement element = elementGrid[x, y, z].ChangePrefab(prefabIndex);
        elementGrid[x, y, z] = element;
        RecreateElement(x, y, z, element);
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

        public CubeGridElement ChangePrefab(int prefabIndex)
        {
            return new(rotation, prefabIndex);
        }

        public GameObject GetPrefab()
        {
            return PrefabHelper.PrefabFromIndex(prefabIndex);
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
