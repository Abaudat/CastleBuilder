using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseProxy : MonoBehaviour
{
    private DatabaseReference firebaseDatabaseReference;

    // Start is called before the first frame update
    void Awake()
    {
        firebaseDatabaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public Task SaveLevelAsync(Level_v1 level)
    {
        return firebaseDatabaseReference.Child("levels").Child("v1").Push().SetRawJsonValueAsync(JsonUtility.ToJson(level));
    }

    public Task<List<Level_v1>> RetrieveAllLevelsAsync()
    {
        return firebaseDatabaseReference.Child("levels").Child("v1").GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot dataSnapshot = task.Result;
            List<Level_v1> levels = new();
            foreach(DataSnapshot child in dataSnapshot.Children)
            {
                levels.Add(JsonUtility.FromJson<Level_v1>(child.GetRawJsonValue()));
            }
            return levels;
        });
    }
}
