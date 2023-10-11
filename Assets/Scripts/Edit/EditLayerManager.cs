using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditLayerManager : MonoBehaviour
{
    public static event EventHandler<LayerChangedEventArgs> LayerChanged;
    public int currentHeight;

    [SerializeField]
    private Slider floorSlider;
    [SerializeField]
    private TMP_Text floorText;

    private CubeGridEditor cubeGridEditor;

    private void Awake()
    {
        cubeGridEditor = FindObjectOfType<CubeGridEditor>();
    }

    void Update()
    {
        if (cubeGridEditor.isEditing)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeCurrentLayerTo(currentHeight + 1);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeCurrentLayerTo(currentHeight - 1);
            }
        }
    }

    protected virtual void OnLayerChanged(LayerChangedEventArgs e)
    {
        LayerChanged?.Invoke(this, e);
    }

    public void ChangeCurrentLayerTo(float newHeightFloat)
    {
        int newHeight = Mathf.FloorToInt(newHeightFloat + 0.5f);
        if (newHeight >= 0 && newHeight < CubeGrid.HEIGHT)
        {
            int oldHeight = currentHeight;
            currentHeight = newHeight;
            floorSlider.value = newHeight;
            floorText.text = (newHeight + 1).ToString();
            OnLayerChanged(new(newHeight, oldHeight));
        }
    }

    public class LayerChangedEventArgs : EventArgs
    {
        public int oldHeight, newHeight;

        public LayerChangedEventArgs(int newHeight, int oldHeight)
        {
            this.newHeight = newHeight;
            this.oldHeight = oldHeight;
        }
    }
}
