using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class FirebaseProxy : MonoBehaviour
{
    private FirebaseRestClient firebaseRestClient;

    // Start is called before the first frame update
    void Awake()
    {
        firebaseRestClient = FindObjectOfType<FirebaseRestClient>();
    }

    public Task SaveLevelAsync(Level_v1 level)
    {
        return firebaseRestClient.SaveLevel(JsonConvert.SerializeObject(level));
    }

    public Task<List<Level_v1>> RetrieveAllLevelsAsync()
    {
        return firebaseRestClient.GetLevels().ContinueWith(task =>
        {
            Dictionary<string, Level_v1> levelList = JsonConvert.DeserializeObject<Dictionary<string, Level_v1>>(task.Result);
            return levelList.Select(x => x.Value).ToList();
        });
    }
}
