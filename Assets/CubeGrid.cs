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

    public GameObject GetInstance(int x, int y, int z)
    {
        if(x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth)
        {
            return instancesGrid[x, y, z];
        }
        Debug.LogWarning("Out of bounds: " + x + " " + y + " " + z);
        return null;
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

    public void AddConsumerToProducer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        elementGrid[producerX, producerY, producerZ].consumerCoords.Add(new Vector3Int(consumerX, consumerY, consumerZ));
        RecreateElement(producerX, producerY, producerZ, elementGrid[producerX, producerY, producerZ]);
    }

    public bool IsElementEmpty(int x, int y, int z)
    {
        return elementGrid[x, y, z].IsEmpty();
    }

    public void SelectElement(int x, int y, int z)
    {
        if(elementGrid[x, y, z].IsEmpty())
        {
            Debug.LogWarning("Element at " + x + " " + y + " " + z + " is empty, cannot select it");
            return;
        }
        previousMaterial = instancesGrid[x, y, z].GetComponentInChildren<Renderer>().material;
        instancesGrid[x, y, z].GetComponentInChildren<Renderer>().material = selectMaterial;
    }

    public void UnselectElement(int x, int y, int z)
    {
        if (elementGrid[x, y, z].IsEmpty())
        {
            Debug.LogWarning("Element at " + x + " " + y + " " + z + " is empty, cannot unselect it");
            return;
        }
        instancesGrid[x, y, z].GetComponentInChildren<Renderer>().material = previousMaterial;
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
                        gameObject.GetComponentInChildren<Renderer>().enabled = false;
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
                    gameObject.GetComponentInChildren<Renderer>().enabled = true;
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
                        gameObject.GetComponentInChildren<Renderer>().enabled = true;
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
                }
            }
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
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
            GameObject gameObject = Instantiate(element.GetPrefab(), new Vector3(x, y, z), element.rotation.ToWorldRot());
            instancesGrid[x, y, z] = gameObject;
            element.consumerCoords.ForEach(coord =>
            {
                SignalConsumer consumer = GetInstance(coord.x, coord.y, coord.z).GetComponentInChildren<SignalConsumer>();
                gameObject.GetComponentInChildren<SignalProducer>().consumers.Add(consumer);
            });
        }
    }

    public class CubeGridElement
    {
        public Rotation rotation;
        public int prefabIndex;
        public List<Vector3Int> consumerCoords;

        public CubeGridElement(Rotation rotation, int prefabIndex)
        {
            this.rotation = rotation;
            this.prefabIndex = prefabIndex;
            this.consumerCoords = new List<Vector3Int>();
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

        public bool IsEmpty()
        {
            return prefabIndex == 0;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)rotation);
            writer.Write(prefabIndex);
            writer.Write(consumerCoords.Count);
            consumerCoords.ForEach(coord =>
            {
                writer.Write(coord.x);
                writer.Write(coord.y);
                writer.Write(coord.z);
            });
        }

        public virtual void Load(BinaryReader reader)
        {
            rotation = (Rotation)reader.ReadByte();
            prefabIndex = reader.ReadInt32();
            int numberOfConsumers = reader.ReadInt32();
            for (int i = 0; i < numberOfConsumers; i++)
            {
                consumerCoords.Add(new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
            }
        }
    }
}
