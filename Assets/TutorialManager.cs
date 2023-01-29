using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject editModeTutorialPanel, blockSelectionPanel, blockPlacementPanel, floorChangePanel, electricityPanel, noChestPanel, playModeTutorial;

    bool blockSelected = false;
    bool blockPlaced = false;
    bool floorChanged = false;

    float playModeTutorialTimeout = -1f;

    private void Update()
    {
        if (playModeTutorialTimeout > 0)
        {
            playModeTutorialTimeout -= Time.deltaTime;
            if (playModeTutorialTimeout <= 0)
            {
                playModeTutorial.SetActive(false);
            }
        }
    }

    public void ToggleEditModeTutorial()
    {
        if (!PlayerPrefs.HasKey("editModeTutorialSeen"))
        {
            PlayerPrefs.SetString("editModeTutorialSeen", "true");
            editModeTutorialPanel.SetActive(true);
        }
    }

    public void ToggleElectricityPanel()
    {
        if (!PlayerPrefs.HasKey("electricityTutorialSeen"))
        {
            PlayerPrefs.SetString("electricityTutorialSeen", "true");
            electricityPanel.SetActive(true);
        }
    }

    public void TogglePlayModeTutorial()
    {
        if (!PlayerPrefs.HasKey("playModeTutorialSeen"))
        {
            PlayerPrefs.SetString("playModeTutorialSeen", "true");
            playModeTutorial.SetActive(true);
            playModeTutorialTimeout = 5f;
        }
    }

    public void ShowNoChestPanel()
    {
        noChestPanel.SetActive(true);
    }

    public void SelectBlock()
    {
        if (!blockSelected && !PlayerPrefs.HasKey("blockPlacmentTutorialSeen"))
        {
            PlayerPrefs.SetString("blockPlacmentTutorialSeen", "true");
            blockSelected = true;
            blockSelectionPanel.SetActive(false);
            blockPlacementPanel.SetActive(true);
        }
    }

    public void PlaceBlock()
    {
        if (!blockPlaced && !PlayerPrefs.HasKey("floorChangeTutorialSeen"))
        {
            PlayerPrefs.SetString("floorChangeTutorialSeen", "true");
            blockPlaced = true;
            blockPlacementPanel.SetActive(false);
            floorChangePanel.SetActive(true);
        }
    }

    public void ChangeFloor()
    {
        if (!floorChanged && !PlayerPrefs.HasKey("floorChangeTutorialDone"))
        {
            PlayerPrefs.SetString("floorChangeTutorialDone", "true");
            floorChanged = true;
            floorChangePanel.SetActive(false);
        }
    }
}
