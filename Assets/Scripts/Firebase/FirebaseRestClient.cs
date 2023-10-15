using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseRestClient : MonoBehaviour
{
    private static readonly string firebaseLevelsEndpoint = "https://castlebuilder-4b9d0-default-rtdb.europe-west1.firebasedatabase.app/levels/v1.json";

    private HttpClient httpClient = new();

    public Task<string> GetLevels()
    {
        return httpClient.GetStringAsync(firebaseLevelsEndpoint);
    }

    public Task<HttpResponseMessage> SaveLevel(string levelJson)
    {
        var content = new StringContent(levelJson, Encoding.UTF8, "application/json");
        return httpClient.PostAsync(firebaseLevelsEndpoint, content);
    }

    private void OnDestroy()
    {
        httpClient.Dispose();
    }
}
