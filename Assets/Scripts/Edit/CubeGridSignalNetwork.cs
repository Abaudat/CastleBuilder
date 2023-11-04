using System.Collections.Generic;
using UnityEngine;

public class CubeGridSignalNetwork : MonoBehaviour
{
    private CubeGridInstanceManager cubeGridInstanceCreator;
    private Dictionary<Vector3Int, List<Vector3Int>> producerToConsumers = new Dictionary<Vector3Int, List<Vector3Int>>();

    private void Start()
    {
        cubeGridInstanceCreator = FindObjectOfType<CubeGridInstanceManager>();
        CubeGrid.GridLoaded += GridLoadedHandler;
        CubeGrid.GridCleared += GridClearedHandler;
        CubeGridInstanceManager.GridInstantiated += GridInstantiatedHandler;
        CubeGrid.ElementConsumerAdded += ConsumerAddedHandler;
        CubeGrid.ElementConsumerRemoved += ConsumerRemovedHandler;
    }

    private void GridLoadedHandler(object sender, CubeGrid.GridLoadedEventArgs gridLoadedEventArgs)
    {
        RecomputeSignalMap(gridLoadedEventArgs.elementGrid);
    }

    private void GridClearedHandler(object sender, CubeGrid.GridClearedEventArgs gridClearedEventArgs)
    {
        RecomputeSignalMap(gridClearedEventArgs.newElementGrid);
    }

    private void GridInstantiatedHandler(object sender, CubeGridInstanceManager.GridInstantiatedEventArgs gridInstantiatedEventArgs)
    {
        PropagateConsumersToProducerInstances(gridInstantiatedEventArgs.instancesGrid);
    }

    private void ConsumerAddedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs elementConsumerModifiedEventArgs)
    {
        AddConsumerToProducer(new Vector3Int(elementConsumerModifiedEventArgs.x, elementConsumerModifiedEventArgs.y, elementConsumerModifiedEventArgs.z), elementConsumerModifiedEventArgs.consumerCoords);
    }

    private void ConsumerRemovedHandler(object sender, CubeGrid.ElementConsumerModifiedEventArgs elementConsumerModifiedEventArgs)
    {
        RemoveConsumerFromProducer(new Vector3Int(elementConsumerModifiedEventArgs.x, elementConsumerModifiedEventArgs.y, elementConsumerModifiedEventArgs.z), elementConsumerModifiedEventArgs.consumerCoords);
    }

    private void AddConsumerToProducer(Vector3Int producerCoords, Vector3Int consumerCoords)
    {
        if (producerToConsumers.TryGetValue(producerCoords, out var currentConsumerList))
        {
            currentConsumerList.Add(consumerCoords);
        }
        else
        {
            producerToConsumers[producerCoords] = new List<Vector3Int>() { consumerCoords };
        }
        SignalProducer producer = cubeGridInstanceCreator.GetInstance(producerCoords.x, producerCoords.y, producerCoords.z).GetComponentInChildren<SignalProducer>();
        SignalConsumer consumer = cubeGridInstanceCreator.GetInstance(consumerCoords.x, consumerCoords.y, consumerCoords.z).GetComponentInChildren<SignalConsumer>();
        producer.consumers.Add(consumer);
    }

    private void RemoveConsumerFromProducer(Vector3Int producerCoords, Vector3Int consumerCoords)
    {
        if (producerToConsumers.TryGetValue(producerCoords, out var currentConsumerList))
        {
            currentConsumerList.Remove(consumerCoords);
        }
        SignalProducer producer = cubeGridInstanceCreator.GetInstance(producerCoords.x, producerCoords.y, producerCoords.z).GetComponentInChildren<SignalProducer>();
        SignalConsumer consumer = cubeGridInstanceCreator.GetInstance(consumerCoords.x, consumerCoords.y, consumerCoords.z).GetComponentInChildren<SignalConsumer>();
        producer.consumers.Remove(consumer);
    }

    private void RecomputeSignalMap(CubeGrid.CubeGridElement[,,] elementGrid)
    {
        producerToConsumers.Clear();
        for (int i = 0; i < CubeGrid.WIDTH; i++)
        {
            for (int j = 0; j < CubeGrid.HEIGHT; j++)
            {
                for (int k = 0; k < CubeGrid.DEPTH; k++)
                {
                    if (elementGrid[i, j, k].consumerCoords.Count > 0)
                    {
                        producerToConsumers.Add(new Vector3Int(i, j, k), elementGrid[i, j, k].consumerCoords);
                    }
                }
            }
        }
    }

    private void PropagateConsumersToProducerInstances(GameObject[,,] instancesGrid)
    {
        foreach(Vector3Int producerCoords in producerToConsumers.Keys)
        {
            GameObject producerInstance = instancesGrid[producerCoords.x, producerCoords.y, producerCoords.z];
            SignalProducer producer = producerInstance.GetComponentInChildren<SignalProducer>();
            producer.consumers.Clear();
            producerToConsumers[producerCoords].ForEach(consumerCoords =>
            {
                SignalConsumer consumer = instancesGrid[consumerCoords.x, consumerCoords.y, consumerCoords.z].GetComponentInChildren<SignalConsumer>();
                producer.consumers.Add(consumer);
            });
        }
    }
}
