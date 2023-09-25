using UnityEngine;
using TMPro;

public class ApplicationVersionDisplayer : MonoBehaviour
{
    public TMP_Text text;

    private void Awake()
    {
        text.text = "ALPHA " + Application.version;
    }
}
