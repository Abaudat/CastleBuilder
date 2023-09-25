using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public static event EventHandler<ElementEventArgs> ElementReplaced;
    public static event EventHandler<ElementEventArgs> ElementRotated;
    public static event EventHandler<ElementConsumerModifiedEventArgs> ElementConsumerAdded;
    public static event EventHandler<ElementConsumerModifiedEventArgs> ElementConsumerRemoved;
    public static event EventHandler<GridLoadedEventArgs> GridLoaded;

    public static int HEIGHT = 5, WIDTH = 10, DEPTH = 10;
    public CubeGridElement[,,] elementGrid;

    void Awake()
    {
        elementGrid = new CubeGridElement[WIDTH, HEIGHT, DEPTH];
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < DEPTH; k++)
                {
                    SetElementEmpty(i, j, k);
                }
            }
        }
    }

    public void SetElementEmpty(int x, int y, int z, bool notify = true)
    {
        ChangeElement(x, y, z, 0, Rotation.NORTH, notify);
    }

    public void RotateElement(int x, int y, int z)
    {
        elementGrid[x, y, z].Rotate();
        OnElementRotated(new(x, y, z, elementGrid[x, y, z]));
    }

    public void ChangeElement(int x, int y, int z, int prefabIndex, Rotation rotation, bool notify = true)
    {
        elementGrid[x, y, z] = new(rotation, prefabIndex);
        if (notify)
        {
            OnElementReplaced(new(x, y, z, elementGrid[x, y, z]));
        }
    }

    public bool AddConsumerToProducer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        bool success = elementGrid[producerX, producerY, producerZ].AddConsumerCoord(consumerX, consumerY, consumerZ);
        if (success)
        {
            OnElementConsumerAdded(new(producerX, producerY, producerZ, new Vector3Int(consumerX, consumerY, consumerZ)));
        }
        return success;
    }

    public bool RemoveConsumerFromProducer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        bool success = elementGrid[producerX, producerY, producerZ].RemoveConsumerCoord(consumerX, consumerY, consumerZ);
        if (success)
        {
            OnElementConsumerRemoved(new(producerX, producerY, producerZ, new Vector3Int(consumerX, consumerY, consumerZ)));
        }
        return success;
    }

    public bool ProducerContainsConsumer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        CubeGridElement element = elementGrid[producerX, producerY, producerZ];
        return element.consumerCoords.Contains(new Vector3Int(consumerX, consumerY, consumerZ));
    }

    public List<Vector3Int> ProducersForConsumer(int consumerX, int consumerY, int consumerZ)
    {
        List<Vector3Int> producers = new List<Vector3Int>();
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < DEPTH; k++)
                {
                    if (ProducerContainsConsumer(consumerX, consumerY, consumerZ, i, j, k))
                    {
                        producers.Add(new Vector3Int(i, j, k));
                    }
                }
            }
        }
        return producers;
    }

    public List<Vector3Int> ConsumersForProducer(int producerX, int producerY, int producerZ)
    {
        return elementGrid[producerX, producerY, producerZ].consumerCoords;
    }

    public bool IsElementEmpty(int x, int y, int z)
    {
        return elementGrid[x, y, z].IsEmpty();
    }

    public bool ContainsAtLeastOneChest()
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < DEPTH; k++)
                {
                    if (elementGrid[i, j, k].prefabIndex == 5)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void Save(BinaryWriter writer)
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < DEPTH; k++)
                {
                    elementGrid[i, j, k].Save(writer);
                }
            }
        }
    }

    public void Load(BinaryReader reader, int version)
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < DEPTH; k++)
                {
                    elementGrid[i, j, k].Load(reader, version);
                }
            }
        }
        OnGridLoaded(new(elementGrid));
    }

    public void ClearAll()
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < DEPTH; k++)
                {
                    SetElementEmpty(i, j, k, false);
                }
            }
        }
        OnGridLoaded(new(elementGrid));
    }

    protected virtual void OnElementReplaced(ElementEventArgs e)
    {
        ElementReplaced?.Invoke(this, e);
    }

    protected virtual void OnElementRotated(ElementEventArgs e)
    {
        ElementRotated?.Invoke(this, e);
    }

    protected virtual void OnElementConsumerAdded(ElementConsumerModifiedEventArgs e)
    {
        ElementConsumerAdded?.Invoke(this, e);
    }

    protected virtual void OnElementConsumerRemoved(ElementConsumerModifiedEventArgs e)
    {
        ElementConsumerRemoved?.Invoke(this, e);
    }

    protected virtual void OnGridLoaded(GridLoadedEventArgs e)
    {
        GridLoaded?.Invoke(this, e);
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

        public void Rotate()
        {
            rotation = rotation.Rotate();
        }

        public bool IsEmpty()
        {
            return prefabIndex == 0;
        }

        public bool AddConsumerCoord(int x, int y, int z)
        {
            Vector3Int vector3Int = new Vector3Int(x, y, z);
            if (!consumerCoords.Contains(vector3Int))
            {
                consumerCoords.Add(vector3Int);
                return true;
            }
            return false;
        }

        public bool RemoveConsumerCoord(int x, int y, int z)
        {
            Vector3Int vector3Int = new Vector3Int(x, y, z);
            if (consumerCoords.Contains(vector3Int))
            {
                return consumerCoords.Remove(vector3Int);
            }
            return false;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)rotation);
            writer.Write((byte)prefabIndex);
            writer.Write((byte)consumerCoords.Count);
            consumerCoords.ForEach(coord =>
            {
                writer.Write((sbyte)coord.x);
                writer.Write((sbyte)coord.y);
                writer.Write((sbyte)coord.z);
            });
        }

        public virtual void Load(BinaryReader reader, int version)
        {
            switch (version)
            {
                case 0:
                    LoadVersion0(reader);
                    break;
                case 1:
                case 2:
                    LoadVersion1(reader);
                    break;
            }
            
        }

        private void LoadVersion0(BinaryReader reader)
        {
            rotation = (Rotation)reader.ReadByte();
            prefabIndex = reader.ReadInt32();
            int numberOfConsumers = reader.ReadInt32();
            for (int i = 0; i < numberOfConsumers; i++)
            {
                consumerCoords.Add(new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
            }
        }

        private void LoadVersion1(BinaryReader reader)
        {
            rotation = (Rotation)reader.ReadByte();
            prefabIndex = reader.ReadByte();
            int numberOfConsumers = reader.ReadByte();
            for (int i = 0; i < numberOfConsumers; i++)
            {
                consumerCoords.Add(new Vector3Int(reader.ReadSByte(), reader.ReadSByte(), reader.ReadSByte()));
            }
        }
    }

    public class ElementEventArgs : EventArgs
    {
        public int x, y, z;
        public CubeGridElement newElement;

        public ElementEventArgs(int x, int y, int z, CubeGridElement newElement)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.newElement = newElement;
        }
    }

    public class ElementConsumerModifiedEventArgs : EventArgs
    {
        public int x, y, z;
        public Vector3Int consumerCoords;

        public ElementConsumerModifiedEventArgs(int x, int y, int z, Vector3Int consumerCoords)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.consumerCoords = consumerCoords;
        }
    }

    public class GridLoadedEventArgs : EventArgs
    {
        public CubeGridElement[,,] elementGrid;

        public GridLoadedEventArgs(CubeGridElement[,,] elementGrid)
        {
            this.elementGrid = elementGrid;
        }
    }
}
