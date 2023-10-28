using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelListEntry : MonoBehaviour, IPointerDownHandler
{
    public TMP_Text levelNameText, levelCreatorText;

    private LevelBrowserManager levelBrowserManager;
    private SoundManager soundManager;

    private Level_v1 associatedLevel;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        levelBrowserManager = FindObjectOfType<LevelBrowserManager>(); // TODO: Pass as populate argument instead if too resource intensive
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        soundManager.PlayButtonClickClip();
        levelBrowserManager.SelectLevel(associatedLevel);
    }

    public void Populate(Level_v1 level)
    {
        associatedLevel = level;
        levelNameText.text = level.levelName;
        levelCreatorText.text = level.creatorUsername;
    }
}
