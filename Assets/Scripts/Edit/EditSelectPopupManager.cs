using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditSelectPopupManager : MonoBehaviour
{
    public GameObject selectPopup;
    public Image popupPreviewImage;
    public TMP_Text popupNameText, popupDescriptionText;

    private CubeGrid cubeGrid;

    private void Awake()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();
    }

    private void Start()
    {
        CubeGridEditor.SelectEditModeStarted += OnElementSelectedHandler;
        CubeGridEditor.SignalEditModeStarted += OnElementSelectedHandler;
        CubeGridEditor.EditModeStopped += OnEditModeStoppedHandler;
        CubeGridEditor.FreeEditModeStarted += OnEditModeStoppedHandler;
    }

    private void OnElementSelectedHandler(object sender, CubeGridEditor.ElementEventArgs elementEventArgs)
    {
        int prefabIndex = cubeGrid.elementGrid[elementEventArgs.x, elementEventArgs.y, elementEventArgs.z].prefabIndex;
        PopulateAndDisplayPopup(prefabIndex);
    }

    private void OnEditModeStoppedHandler(object sender, EventArgs eventArgs)
    {
        HidePopup();
    }

    private void PopulateAndDisplayPopup(int prefabIndex)
    {
        GameObject typicalInstance = PrefabHelper.PrefabFromIndex(prefabIndex);
        RuntimePreviewGenerator.GenerateModelPreviewAsync(tex => {
            Sprite previewSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            popupPreviewImage.sprite = previewSprite;
        }, typicalInstance.transform);
        popupNameText.SetText(typicalInstance.GetComponent<ElementDescriptor>().elementName);
        popupDescriptionText.SetText(typicalInstance.GetComponent<ElementDescriptor>().elementDescription);
        selectPopup.SetActive(true);
    }

    private void HidePopup()
    {
        selectPopup.SetActive(false);
    }
}
