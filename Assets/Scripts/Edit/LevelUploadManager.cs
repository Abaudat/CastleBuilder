using TMPro;
using UnityEngine;

public class LevelUploadManager : MonoBehaviour
{
    public TMP_InputField levelNameInput, levelDescriptionInput;
    public TMP_Text levelNameInputPlaceholder, levelDescriptionInputPlaceholder;

    private CubeGridPersistenceManager cubeGridPersistenceManager;
    private UsernameManager usernameManager;
    private FirebaseProxy firebaseProxy;

    private void Awake()
    {
        cubeGridPersistenceManager = FindObjectOfType<CubeGridPersistenceManager>();
        usernameManager = FindObjectOfType<UsernameManager>();
        firebaseProxy = FindObjectOfType<FirebaseProxy>();
    }

    public void UploadLevel()
    {
        if(levelNameInput.text == "")
        {
            levelNameInputPlaceholder.text = "Enter a name!";
            return;
        }
        if (levelDescriptionInput.text == "")
        {
            levelDescriptionInputPlaceholder.text = "Enter a description!";
            return;
        }
        Level_v1 level = new Level_v1(cubeGridPersistenceManager.currentLevelCode, usernameManager.GetUsername(), levelDescriptionInput.text, levelNameInput.text); //TODO: Validate inputs
        firebaseProxy.SaveLevelAsync(level);
    }
}
