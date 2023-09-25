using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UiBuildElement : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private int elementPrefabIndex;
    [SerializeField]
    private string elementTitle, elementDescription;
    private CubeGridEditor cubeGridEditor;
    private SoundManager soundManager;
    private TutorialManager tutorialManager;
    private EditTooltipManager editTooltipManager;
    private RectTransform thisRectTransform;

    private static readonly float TOOLTIP_DISPLAY_HOVER_TIME_SECONDS = 1;

    private Sprite previewSprite;
    private float hoverTimeSeconds = 0;
    private bool isHovering = false;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        soundManager = FindObjectOfType<SoundManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        editTooltipManager = FindObjectOfType<EditTooltipManager>();
        thisRectTransform = GetComponent<RectTransform>();
        Image image = GetComponentInChildren<Image>();
        RuntimePreviewGenerator.GenerateModelPreviewAsync(tex => {
            previewSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            image.sprite = previewSprite;
        }, PrefabHelper.PrefabFromIndex(elementPrefabIndex).transform);
        TMP_Text title = GetComponentInChildren<TMP_Text>();
        title.SetText(elementTitle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cubeGridEditor.ChangeCurrentPrefabIndex(elementPrefabIndex);
        soundManager.PlaySelectClip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        hoverTimeSeconds = 0;
        editTooltipManager.HideTooltip();
    }

    private void Update()
    {
        if (isHovering)
        {
            hoverTimeSeconds += Time.deltaTime;
        }
        if (hoverTimeSeconds > TOOLTIP_DISPLAY_HOVER_TIME_SECONDS)
        {
            editTooltipManager.PopulateTooltip(previewSprite, elementTitle, elementDescription);
            editTooltipManager.DisplayTooltip(thisRectTransform);
        }
    }
}
