using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class PersistenceHelpers
{
    public static byte[] compressBytes(byte[] bytes)
    {
        var memory = new byte[bytes.Length];
        bool compressionSuccess = BrotliEncoder.TryCompress(bytes, memory, out int bytesWritten);
        if (!compressionSuccess)
        {
            Debug.LogError("Compression failure, map not exported");
        }
        return memory.Take(bytesWritten).ToArray();
    }

    public static byte[] decompressBytes(byte[] bytes)
    {
        var memory = new byte[15000];
        bool compressionSuccess = BrotliDecoder.TryDecompress(bytes, memory, out int bytesWritten);
        if (!compressionSuccess)
        {
            Debug.LogError("Decompression failure, map not imported");
        }
        return memory.Take(bytesWritten).ToArray();
    }

    public static byte[] addFlag(byte[] bytes, int flag)
    {
        byte[] bytesWithFlag = new byte[bytes.Length + 4];
        byte[] flagBytes = BitConverter.GetBytes(flag);
        flagBytes.CopyTo(bytesWithFlag, 0);
        bytes.CopyTo(bytesWithFlag, 4);
        return bytesWithFlag;
    }

    public static string bytesToStringLevelCode(byte[] bytes)
    {
        return System.Convert.ToBase64String(bytes).Trim('=');
    }

    public static byte[] stringLevelCodeToBytes(string levelCode)
    {
        return System.Convert.FromBase64String(inferBase64Padding(levelCode));
    }

    private static string inferBase64Padding(string nonPaddedBase64String)
    {
        switch(nonPaddedBase64String.Length % 4)
        {
            case 3:
                nonPaddedBase64String += "=";
                break;
            case 2:
                nonPaddedBase64String += "==";
                break;
        }
        return nonPaddedBase64String;
    }
}
