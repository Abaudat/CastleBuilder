using UnityEngine;
using TMPro;

public class UsernameManager : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public GameObject usernamePanel;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("username"))
        {
            usernamePanel.SetActive(true);
        }
    }

    public void SetUsername()
    {
        if (!PlayerPrefs.HasKey("username"))
        {
            PlayerPrefs.SetString("username", usernameInputField.text); //TODO: Add validation for username (only characters that can go in Firebase, no empty strings)
        }
    }

    public string GetUsername()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            return PlayerPrefs.GetString("username");
        }
        return "Unknown";
    }
}
