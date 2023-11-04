using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class VolumeSlider : MonoBehaviour, IPointerUpHandler
{
    public TMP_Text sliderLabel;
    public Slider slider;

    private SoundManager soundManager;
    private OptionsManager optionsManager;
    private AudioSource soundManagerAudioSource;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        optionsManager = FindObjectOfType<OptionsManager>();
        soundManagerAudioSource = soundManager.gameObject.GetComponent<AudioSource>();
    }

    private void Start()
    {
        OnValueChange(optionsManager.GetSoundVolume());
        slider.value = optionsManager.GetSoundVolume();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        soundManager.PlayButtonClickClip();
    }

    public void OnValueChange(float value)
    {
        soundManagerAudioSource.volume = value / 100;
        sliderLabel.text = Mathf.CeilToInt(value).ToString();
        optionsManager.SetSoundVolume(value);
    }
}
