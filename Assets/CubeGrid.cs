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

    public bool AddConsumerToProducer(int consumerX, int consumerY, int consumerZ, int producerX, int producerY, int producerZ)
    {
        bool result = elementGrid[producerX, producerY, producerZ].AddConsumerCoord(consumerX, consumerY, consumerZ);
        RecomputeSignalNetwork();
        return result;
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

    public List<Vector3Int> ProducersForConsumer(int consumerX, int consumerY, int consumerZ)
    {
        List<Vector3Int> producers = new List<Vector3Int>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
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

    public void SetPlacementModeMaterials(int currentLayerY)
    {
        ChangeMaterialsOfLayersBelow(x => x.Shadow(), currentLayerY);
        ChangeMaterialsOfLayer(x => x.Reset(), currentLayerY);
        ChangeMaterialsOfLayer(x => x.Transparent(), currentLayerY + 1);
        ChangeMaterialsOfLayersAbove(x => x.Invisible(), currentLayerY + 1);
    }

    public void SetSignalModeMaterials(int currentLayerY, Vector3Int? currentProducerCoords, Vector3Int? currentConsumerCoords, Vector3Int currentHovered)
    {
        ChangeMaterialsOfLayersBelow(x => x.Shadow(), currentLayerY + 1);
        ChangeMaterialsOfLayer(x => x.Transparent(), currentLayerY + 1);
        ChangeMaterialsOfLayersAbove(x => x.Invisible(), currentLayerY + 1);
        if (currentProducerCoords.HasValue)
        {
            ChangeAllConsumersUnderMaterials(currentLayerY, k => k.HighlightUnlinkedUnder());
            ChangeAllConsumersSameLayerMaterials(currentLayerY, k => k.HighlightUnlinked());
            ChangeMaterial(currentHovered.x, currentHovered.y, currentHovered.z, k => k.Select());
            ChangeAllLinkedConsumersUnderMaterials(currentLayerY, currentProducerCoords.Value.x, currentProducerCoords.Value.y, currentProducerCoords.Value.z, k => k.HighlightLinkedUnder());
            ChangeAllLinkedConsumersSameLayerMaterials(currentLayerY, currentProducerCoords.Value.x, currentProducerCoords.Value.y, currentProducerCoords.Value.z, k => k.HighlightLinked());
            ChangeAllProducersSameLayerMaterials(currentLayerY, k => k.HighlightSignal());
            ChangeMaterial(currentProducerCoords.Value.x, currentProducerCoords.Value.y, currentProducerCoords.Value.z, k => k.Select());
        }
        else if (currentConsumerCoords.HasValue)
        {
            ChangeAllProducersUnderMaterials(currentLayerY, k => k.HighlightUnlinkedUnder());
            ChangeAllProducersSameLayerMaterials(currentLayerY, k => k.HighlightUnlinked());
            ChangeMaterial(currentHovered.x, currentHovered.y, currentHovered.z, k => k.Select());
            ChangeAllLinkedProducersUnderMaterials(currentLayerY, currentConsumerCoords.Value.x, currentConsumerCoords.Value.y, currentConsumerCoords.Value.z, k => k.HighlightLinkedUnder());
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
            ChangeMaterial(currentHovered.x, currentHovered.y, currentHovered.z, k => k.Select());
        }
    }

    public void ChangeMaterial(int x, int y, int z, Action<DisplayManager> action)
    {
        if (IsInBounds(x, y, z) && elementGrid[x, y, z].IsEmpty())
        {
            return;
        }
        foreach(DisplayManager displayManager in instancesGrid[x, y, z].GetComponentsInChildren<DisplayManager>())
        {
            action.Invoke(displayManager);
        }
    }

    public void ChangeAllMaterials(Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (_, _, _) => true);
    }

    public bool ContainsAtLeastOneChest()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
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

    private void ChangeAllMaterials(Action<DisplayManager> action, Func<int, int, int, bool> filter)
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

    private void ChangeMaterialsOfLayer(Action<DisplayManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y == layerY);
    }

    private void ChangeMaterialsOfLayersBelow(Action<DisplayManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y < layerY);
    }

    private void ChangeMaterialsOfLayersAbove(Action<DisplayManager> action, int layerY)
    {
        ChangeAllMaterials(action, (_, y, _) => y > layerY);
    }

    private void ChangeAllConsumersUnderMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && consumersGrid[x, y, z] != null);
    }

    private void ChangeAllProducersUnderMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && producersGrid[x, y, z] != null);
    }

    private void ChangeAllConsumersSameLayerMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && consumersGrid[x, y, z] != null);
    }

    private void ChangeAllProducersSameLayerMaterials(int layerY, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && producersGrid[x, y, z] != null);
    }

    private void ChangeAllLinkedConsumersUnderMaterials(int layerY, int producerX, int producerY, int producerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && elementGrid[producerX, producerY, producerZ].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(x, y, z)));
    }

    private void ChangeAllLinkedConsumersSameLayerMaterials(int layerY, int producerX, int producerY, int producerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && elementGrid[producerX, producerY, producerZ].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(x, y, z)));
    }

    private void ChangeAllLinkedProducersUnderMaterials(int layerY, int consumerX, int consumerY, int consumerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y < layerY && elementGrid[x, y, z].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(consumerX, consumerY, consumerZ)));
    }

    private void ChangeAllLinkedProducersSameLayerMaterials(int layerY, int consumerX, int consumerY, int consumerZ, Action<DisplayManager> action)
    {
        ChangeAllMaterials(action, (x, y, z) => y == layerY && elementGrid[x, y, z].consumerCoords.Any(consumerCoords => consumerCoords == new Vector3Int(consumerX, consumerY, consumerZ)));
    }

    public void ClearAll()
    {
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
        RecomputeSignalNetwork();
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

    public void Load(BinaryReader reader, int version)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    elementGrid[i, j, k].Load(reader, version);
                }
            }
        }
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

    private void TriggerAllSelfdestructs()
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

    public void PrepareForPlay()
    {
        RecreateAllElements();
        EnableAllRigidbodies();
        TriggerAllSelfdestructs();
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

    public bool IsInBounds(int x, int y, int z)
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
}
