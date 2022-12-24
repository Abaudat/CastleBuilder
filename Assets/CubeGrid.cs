using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public Material selectMaterial;
    public int height = 5, width = 10, depth = 10;
    CubeGridElement[,,] elementGrid;
    GameObject[,,] instancesGrid;
    GameObject[,,] producersGrid;
    GameObject[,,] consumersGrid;

    private void Awake()
    {
        instancesGrid = new GameObject[width, height, depth];
        producersGrid = new GameObject[width, height, depth];
        consumersGrid = new GameObject[width, height, depth];
        elementGrid = new CubeGridElement[width, height, depth];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    SetElementEmpty(i, j, k);
                }
            }
        }
    }

    public void SetElementEmpty(int x, int y, int z)
    {
        ReplaceElement(x, y, z, new(Rotation.NORTH, 0));
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
        elementGrid[x, y, z] = elementGrid[x, y, z].Rotate();
        RecreateSameElement(x, y, z);
    }

    public void ChangeElement(int x, int y, int z, int prefabIndex, Rotation rotation)
    {
        CubeGridElement element = elementGrid[x, y, z].ChangePrefabAndRotation(prefabIndex, rotation);
        ReplaceElement(x, y, z, element);
    }

    public void AddConsumerToProducer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        elementGrid[producerX, producerY, producerZ].AddConsumerCoord(consumerX, consumerY, consumerZ);
        RecomputeSignalNetwork();
    }

    public void RemoveConsumerFromProducer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        elementGrid[producerX, producerY, producerZ].RemoveConsumerCoord(consumerX, consumerY, consumerZ);
        RecomputeSignalNetwork();
    }

    public bool ProducerContainsConsumer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        CubeGridElement element = elementGrid[producerX, producerY, producerZ];
        return element.consumerCoords.Contains(new Vector3Int(consumerX, consumerY, consumerZ));
    }

    public bool IsElementEmpty(int x, int y, int z)
    {
        return elementGrid[x, y, z].IsEmpty();
    }

    public void SetPlacementModeMaterials(int currentLayerY)
    {
        ChangeMaterialsOfLayersBelow(x => x.Shadow(), currentLayerY);
        ChangeMaterialsOfLayer(x => x.ResetMaterial(), currentLayerY);
        ChangeMaterialsOfLayer(x => x.Transparent(), currentLayerY + 1);
        ChangeMaterialsOfLayersAbove(x => x.Invisible(), currentLayerY + 1);
    }

    public void SetSignalModeMaterials(int currentLayerY, Vector3Int? currentProducerCoords, Vector3Int? currentConsumerCoords)
    {
        ChangeMaterialsOfLayersBelow(x => x.Shadow(), currentLayerY + 1);
        ChangeMaterialsOfLayer(x => x.Transparent(), currentLayerY + 1);
        ChangeMaterialsOfLayersAbove(x => x.Invisible(), currentLayerY + 1);
        if (currentProducerCoords.HasValue)
        {
            ChangeAllConsumersUnderMaterials(currentLayerY, k => k.HighlightUnlinkedUnder());
            ChangeAllLinkedConsumersUnderMaterials(currentLayerY, currentProducerCoords.Value.x, currentProducerCoords.Value.y, currentProducerCoords.Value.z, k => k.HighlightLinkedUnder());
            ChangeAllConsumersSameLayerMaterials(currentLayerY, k => k.HighlightUnlinked());
            ChangeAllLinkedConsumersSameLayerMaterials(currentLayerY, currentProducerCoords.Value.x, currentProducerCoords.Value.y, currentProducerCoords.Value.z, k => k.HighlightLinked());
            ChangeAllProducersSameLayerMaterials(currentLayerY, k => k.HighlightSignal());
            ChangeMaterial(currentProducerCoords.Value.x, currentProducerCoords.Value.y, currentProducerCoords.Value.z, k => k.Select());
        }
        else if (currentConsumerCoords.HasValue)
        {
            ChangeAllProducersUnderMaterials(currentLayerY, k => k.HighlightUnlinkedUnder());
            ChangeAllLinkedProducersUnderMaterials(currentLayerY, currentConsumerCoords.Value.x, currentConsumerCoords.Value.y, currentConsumerCoords.Value.z, k => k.HighlightLinkedUnder());
            ChangeAllProducersSameLayerMaterials(currentLayerY, k => k.HighlightUnlinked());
            ChangeAllLinkedProducersSameLayerMaterials(currentLayerY, currentConsumerCoords.Value.x, currentConsumerCoords.Value.y, currentConsumerCoords.Value.z, k => k.HighlightLinked());
            ChangeAllConsumersSameLayerMaterials(currentLayerY, k => k.HighlightSignal());
            ChangeMaterial(currentConsumerCoords.Value.x, currentConsumerCoords.Value.y, currentConsumerCoords.Value.z, k => k.Select());
        }
        else
        {
            ChangeAllConsumersUnderMaterials(currentLayerY, k => k.HighlightSignalUnder());
            ChangeAllProducersUnderMaterials(currentLayerY, k => k.HighlightSignalUnder());
            ChangeAllConsumersSameLayerMaterials(currentLayerY, k => k.HighlightSignal());
            ChangeAllProducersSameLayerMaterials(currentLayerY, k => k.HighlightSignal());
        }
    }

    public void ChangeMaterial(int x, int y, int z, Action<MaterialManager> action)
    {
        if (IsInBounds(x, y, z) && elementGrid[x, y, z].IsEmpty())
        {
            return;
        }
        action.Invoke(instancesGrid[x, y, z].GetComponentInChildren<MaterialManager>());
    }

    public void ChangeAllMaterials(Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (_, _, _) => true);
    }

    private void ChangeAllMaterials(Action<MaterialManager> action, Func<int, int, int, bool> filter)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (filter.Invoke(i, j, k))
                    {
                        ChangeMaterial(i, j, k, action);
                    }
                }
            }
        }
    }

    private void ChangeMaterialsOfLayer(Action<MaterialManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y == layerY);
    }

    private void ChangeMaterialsOfLayersBelow(Action<MaterialManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y < layerY);
    }

    private void ChangeMaterialsOfLayersAbove(Action<MaterialManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y > layerY);
    }

    private void ChangeAllConsumersUnderMaterials(int layerY, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && consumersGrid[x, y, z] != null);
    }

    private void ChangeAllProducersUnderMaterials(int layerY, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && producersGrid[x, y, z] != null);
    }

    private void ChangeAllConsumersSameLayerMaterials(int layerY, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && consumersGrid[x, y, z] != null);
    }

    private void ChangeAllProducersSameLayerMaterials(int layerY, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && producersGrid[x, y, z] != null);
    }

    private void ChangeAllLinkedConsumersUnderMaterials(int layerY, int producerX, int producerY, int producerZ, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && elementGrid[producerX, producerY, producerZ].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(x, y, z)));
    }

    private void ChangeAllLinkedConsumersSameLayerMaterials(int layerY, int producerX, int producerY, int producerZ, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && elementGrid[producerX, producerY, producerZ].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(x, y, z)));
    }

    private void ChangeAllLinkedProducersUnderMaterials(int layerY, int consumerX, int consumerY, int consumerZ, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && elementGrid[x, y, z].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(consumerX, consumerY, consumerZ)));
    }

    private void ChangeAllLinkedProducersSameLayerMaterials(int layerY, int consumerX, int consumerY, int consumerZ, Action<MaterialManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && elementGrid[x, y, z].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(consumerX, consumerY, consumerZ)));
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
                    RecreateSameElement(i, j, k);
                }
            }
        }
    }

    private void EnableAllRigidbodies()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
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

    public void PrepareForPlay()
    {
        RecreateAllElements();
        EnableAllRigidbodies();
    }

    public void RecreateAllElements()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    RecreateElement(i, j, k);
                }
            }
        }
        RecomputeSignalNetwork();
    }

    private void RecreateSameElement(int x, int y, int z)
    {
        RecreateElement(x, y, z);
        RecomputeSignalNetwork();
    }

    private void ReplaceElement(int x, int y, int z, CubeGridElement element)
    {
        elementGrid[x, y, z] = element;
        RecreateElement(x, y, z);
        RemoveFromAllProducers(x, y, z);
    }

    private void RecreateElement(int x, int y, int z)
    {
        CubeGridElement element = elementGrid[x, y, z];
        if (instancesGrid[x, y, z] != null)
        {
            Destroy(instancesGrid[x, y, z]);
        }
        if (element.GetPrefab() != null)
        {
            GameObject gameObject = Instantiate(element.GetPrefab(), new Vector3(x, y, z), element.rotation.ToWorldRot());
            instancesGrid[x, y, z] = gameObject;
            if (gameObject.TryGetComponentInChildren<SignalProducer>() != null)
            {
                producersGrid[x, y, z] = gameObject;
            }
            else
            {
                producersGrid[x, y, z] = null;
            }
            if (gameObject.TryGetComponentInChildren<SignalConsumer>() != null)
            {
                consumersGrid[x, y, z] = gameObject;
            }
            else
            {
                consumersGrid[x, y, z] = null;
            }
        }
    }

    private void RemoveFromAllProducers(int consumerX, int consumerY, int consumerZ)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    CubeGridElement element = elementGrid[i, j, k];
                    if (element != null && ProducerContainsConsumer(consumerX, consumerY, consumerZ, i, j, k))
                    {
                        RemoveConsumerFromProducer(consumerX, consumerY, consumerZ, i, j, k);
                    }
                }
            }
        }
        RecomputeSignalNetwork();
    }

    private void RewireToConsumers(CubeGridElement element)
    {
        element.consumerCoords.ForEach(coord =>
        {
            SignalConsumer consumer = GetInstance(coord.x, coord.y, coord.z).GetComponentInChildren<SignalConsumer>();
            gameObject.GetComponentInChildren<SignalProducer>().consumers.Add(consumer);
        });
    }

    private void RecomputeSignalNetwork()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    GameObject instance = producersGrid[i, j, k];
                    if (instance != null)
                    {
                        instance.GetComponentInChildren<SignalProducer>().consumers.Clear();
                        elementGrid[i, j, k].consumerCoords.ForEach(coord =>
                        {
                            SignalConsumer consumer = GetInstance(coord.x, coord.y, coord.z).GetComponentInChildren<SignalConsumer>();
                            instance.GetComponentInChildren<SignalProducer>().consumers.Add(consumer);
                        });
                    }
                }
            }
        }
    }

    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth;
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

        public void AddConsumerCoord(int x, int y, int z)
        {
            Vector3Int vector3Int = new Vector3Int(x, y, z);
            if (!consumerCoords.Contains(vector3Int))
            {
                consumerCoords.Add(vector3Int);
            }
        }

        public void RemoveConsumerCoord(int x, int y, int z)
        {
            Vector3Int vector3Int = new Vector3Int(x, y, z);
            if (consumerCoords.Contains(vector3Int))
            {
                consumerCoords.Remove(vector3Int);
            }
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
