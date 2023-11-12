using UnityEngine;
using TMPro;

public class RebindableAction : MonoBehaviour
{
    public TMP_Text bindingText;

    public string actionName;
    public int index;

    private RebindingManager rebindingManager;

    private void Awake()
    {
        rebindingManager = FindObjectOfType<RebindingManager>();
    }

    private void Start()
    {
        UpdateBindingText();
    }

    public void Rebind()
    {
        bindingText.text = "Press any key...";
        rebindingManager.RebindAction(actionName, index, UpdateBindingText);
    }

    private void UpdateBindingText()
    {
        bindingText.text = rebindingManager.GetCurrentBinding(actionName, index);
    }
}
