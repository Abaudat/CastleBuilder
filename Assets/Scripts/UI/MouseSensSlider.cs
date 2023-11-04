using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSensSlider : MonoBehaviour
{
    public TMP_Text sliderLabel;
    public Slider slider;

    private OptionsManager optionsManager;

    private void Awake()
    {
        optionsManager = FindObjectOfType<OptionsManager>();
    }

    private void Start()
    {
        OnValueChange(optionsManager.GetMouseSens());
        slider.value = optionsManager.GetMouseSens();
    }

    public void OnValueChange(float value)
    {
        sliderLabel.text = value.ToString("0.00");
        optionsManager.SetMouseSens(value);
    }
}
