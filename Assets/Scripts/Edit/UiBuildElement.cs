using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UiBuildElement : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private int elementPrefabIndex;
    private CubeGridEditor cubeGridEditor;
    private SoundManager soundManager;
    private EditTooltipManager editTooltipManager;
    private RectTransform thisRectTransform;

    private static readonly float TOOLTIP_DISPLAY_HOVER_TIME_SECONDS = 1;

    private Sprite previewSprite;
    private string previewTitle, previewDescription;
    private float hoverTimeSeconds = 0;
    private bool isHovering = false;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
        soundManager = FindObjectOfType<SoundManager>();
        editTooltipManager = FindObjectOfType<EditTooltipManager>();
        thisRectTransform = GetComponent<RectTransform>();
        Image image = GetComponentInChildren<Image>();
        GameObject typicalInstance = PrefabHelper.PrefabFromIndex(elementPrefabIndex);
        RuntimePreviewGenerator.GenerateModelPreviewAsync(tex => {
            previewSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            image.sprite = previewSprite;
        }, typicalInstance.transform);
        previewTitle = typicalInstance.GetComponent<ElementDescriptor>().elementName;
        previewDescription = typicalInstance.GetComponent<ElementDescriptor>().elementDescription;
        TMP_Text title = GetComponentInChildren<TMP_Text>();
        title.SetText(previewTitle);
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
            editTooltipManager.PopulateTooltip(previewSprite, previewTitle, previewDescription);
            editTooltipManager.DisplayTooltip(thisRectTransform);
        }
    }
}
