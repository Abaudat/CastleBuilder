using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelBrowserManager : MonoBehaviour
{
    public GameObject levelListEntryPrefab, levelDetailsPanel, levelBrowserPanel, editCamera;
    public TMP_Text levelNameText, levelCreatorText, levelDescriptionText;
    public Transform levelList;

    public string selectedLevelCode;

    private FirebaseProxy firebaseProxy;

    private List<GameObject> instantiatedLevelListEntries = new List<GameObject>();

    private void Awake()
    {
        firebaseProxy = FindObjectOfType<FirebaseProxy>();
    }

    private void Start()
    {
        PopulateLevelBrowser();
    }

    public void Browse()
    {
        levelBrowserPanel.SetActive(true);
        editCamera.SetActive(true);
    }

    public void SelectLevel(Level_v1 level)
    {
        levelDetailsPanel.SetActive(true);
        levelNameText.text = level.levelName;
        levelCreatorText.text = level.creatorUsername;
        levelDescriptionText.text = level.description;
        selectedLevelCode = level.levelCode;
    }

    public void PopulateLevelBrowser()
    {
        levelDetailsPanel.SetActive(false);
        foreach (GameObject levelListEntry in instantiatedLevelListEntries)
        {
            Destroy(levelListEntry); //TODO add pooling
        }
        firebaseProxy.RetrieveAllLevelsWithCallback(PopulateFromLevelList);
    }

    private void PopulateFromLevelList(List<Level_v1> levels)
    {
        foreach (Level_v1 level in levels)
        {
            GameObject levelListEntryGameObject = Instantiate(levelListEntryPrefab, levelList);
            LevelListEntry levelListEntry = levelListEntryGameObject.GetComponent<LevelListEntry>();
            levelListEntry.Populate(level);
            instantiatedLevelListEntries.Add(levelListEntryGameObject);
        }
    }
}
