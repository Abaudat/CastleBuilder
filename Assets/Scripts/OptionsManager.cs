using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public void SetSoundVolume(float volume)
    {
        PlayerPrefs.SetFloat("Options_soundVolume", volume);
    }

    public float GetSoundVolume()
    {
        if (PlayerPrefs.HasKey("Options_soundVolume"))
        {
            return PlayerPrefs.GetFloat("Options_soundVolume");
        }
        else return 100f;
    }

    public void SetMouseSens(float sens)
    {
        PlayerPrefs.SetFloat("Options_mouseSens", sens);
    }

    public float GetMouseSens()
    {
        if (PlayerPrefs.HasKey("Options_mouseSens"))
        {
            return PlayerPrefs.GetFloat("Options_mouseSens");
        }
        else return 1f;
    }
}
