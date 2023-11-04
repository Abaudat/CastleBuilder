using System.Collections;
using System.Collections.Generic;
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
        else return 100;
    }
}
