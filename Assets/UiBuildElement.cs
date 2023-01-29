using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiBuildElement : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private int elementPrefabIndex;
    private CubeGridEditor cubeGridEditor;
    private Image image;
    private SoundManager soundManager;
    private TutorialManager tutorialManager;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        soundManager = FindObjectOfType<SoundManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        image = GetComponent<Image>();
        RuntimePreviewGenerator.GenerateModelPreviewAsync(tex => image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f), 
            PrefabHelper.PrefabFromIndex(elementPrefabIndex).transform);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cubeGridEditor.ChangeCurrentPrefabIndex(elementPrefabIndex);
        soundManager.PlaySelectClip();
        tutorialManager.SelectBlock();
    }
}
