using System;
using System.Linq;
using System.IO;
using TMPro;
using UnityEngine;

public class CubeGridPersistenceManager : MonoBehaviour
{
    public TMP_InputField exportInput, importInput;

    private CubeGrid cubeGrid;

    private void Awake()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();
    }

    public void Export()
    {
        using MemoryStream memoryStream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(memoryStream);
        cubeGrid.Save(writer);
        byte[] compressedBytes = PersistenceHelpers.compressBytes(memoryStream.ToArray());
        byte[] bytesWithFlag = PersistenceHelpers.addFlag(compressedBytes, 2);
        exportInput.text = PersistenceHelpers.bytesToStringLevelCode(bytesWithFlag);
    }

    public void Import()
    {
        string importString = importInput.text;
        byte[] bytes = PersistenceHelpers.stringLevelCodeToBytes(importString);
        int format = BitConverter.ToInt32(bytes.Take(4).ToArray());
        BinaryReader reader;
        if (format == 0 || format == 1)
        {
            reader = new BinaryReader(new MemoryStream(bytes.Skip(4).ToArray()));
        }
        else if (format == 2)
        {
            reader = new BinaryReader(new MemoryStream(PersistenceHelpers.decompressBytes(bytes.Skip(4).ToArray())));
        }
        else
        {
            Debug.LogWarning("Unknown map format " + format);
            return;
        }
        cubeGrid.Load(reader, format);
    }
}
