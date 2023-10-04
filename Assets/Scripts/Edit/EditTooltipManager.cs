using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditTooltipManager : MonoBehaviour
{
    public GameObject editTooltip;
    public Image editTooltipImage;
    public TMP_Text editTooltipTitle, editTooltipDescription;
    public RectTransform mainCanvas;

    public void PopulateTooltip(Sprite sprite, string title, string description)
    {
        editTooltipImage.sprite = sprite;
        editTooltipTitle.text = title;
        editTooltipDescription.text = description;
    }

    public void DisplayTooltip(RectTransform element)
    {
        editTooltip.SetActive(true);
        editTooltip.transform.position = ComputeTooltipPosition(element);
    }

    public void HideTooltip()
    {
        editTooltip.SetActive(false);
    }

    private Vector2 ComputeTooltipPosition(RectTransform elementRectTransform)
    {
        RectTransform thisRectTransform = editTooltip.GetComponent<RectTransform>();
        Vector2 topLeftCornerOfElement = new Vector2(elementRectTransform.position.x - elementRectTransform.rect.width / 2, elementRectTransform.position.y + elementRectTransform.rect.height / 2);
        Vector2 idealTooltipPosition = new Vector2(topLeftCornerOfElement.x + thisRectTransform.rect.width / 2, topLeftCornerOfElement.y + thisRectTransform.rect.height / 2);
        Vector2 clampedTooltipPosition = KeepFullyOnScreen(idealTooltipPosition, thisRectTransform);
        return clampedTooltipPosition;
    }

    private Vector2 KeepFullyOnScreen(Vector2 newPos, RectTransform thisRectTransform)
    {
        float minX = (mainCanvas.sizeDelta.x - thisRectTransform.sizeDelta.x) * -0.5f;
        float maxX = (mainCanvas.sizeDelta.x - thisRectTransform.sizeDelta.x) * 0.5f;
        float minY = (mainCanvas.sizeDelta.y - thisRectTransform.sizeDelta.y) * -0.5f;
        float maxY = (mainCanvas.sizeDelta.y - thisRectTransform.sizeDelta.y) * 0.5f;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        return newPos;
    }

}
