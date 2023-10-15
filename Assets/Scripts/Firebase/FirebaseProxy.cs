using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

public class FirebaseProxy : MonoBehaviour
{
    private static readonly string firebaseLevelsEndpoint = "https://castlebuilder-4b9d0-default-rtdb.europe-west1.firebasedatabase.app/levels/v1.json";

    public void SaveLevelAsync(Level_v1 level)
    {
        StartCoroutine(SaveLevelCoroutine(level));
    }

    public void RetrieveAllLevelsWithCallback(Action<List<Level_v1>> callback)
    {
        StartCoroutine(RetrieveAllLevelsCoroutine(callback));
    }

    private IEnumerator RetrieveAllLevelsCoroutine(Action<List<Level_v1>> callback)
    {
        using UnityWebRequest request = UnityWebRequest.Get(firebaseLevelsEndpoint);

        yield return request.SendWebRequest();

        if (!request.result.Equals(UnityWebRequest.Result.Success))
        {
            Debug.LogError("Could not load levels: " + request.error);
        }

        Dictionary<string, Level_v1> levelsDict = JsonConvert.DeserializeObject<Dictionary<string, Level_v1>>(request.downloadHandler.text);
        callback(levelsDict.Select(x => x.Value).ToList());
    }

    private IEnumerator SaveLevelCoroutine(Level_v1 level)
    {
        // Need to instantiate and populate UnityWebRequest manually, Post method doesn't work properly
        using UnityWebRequest request = new UnityWebRequest(firebaseLevelsEndpoint, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(level)));
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (!request.result.Equals(UnityWebRequest.Result.Success))
        {
            Debug.LogError("Could not save level: " + request.error);
        }
    }
}
