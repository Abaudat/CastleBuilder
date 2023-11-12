using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class RebindingManager : MonoBehaviour
{
    InputActionAsset inputActions;

    private RebindingOperation rebindingOperation = null;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        inputActions = playerInput.actions;
        if (PlayerPrefs.HasKey("rebinds"))
        {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
        }
    }

    public string GetCurrentBinding(string actionName, int index)
    {
        if (index == 0)
        {
            return inputActions.FindAction(actionName).GetBindingDisplayString();
        }
        else 
        {
            return inputActions.FindAction(actionName).bindings[index].ToDisplayString();
        }
    }

    public void RebindAction(string actionName, int index, Action callback)
    {
        InputAction action = inputActions.FindAction(actionName);
        action.Disable();
        rebindingOperation = action.PerformInteractiveRebinding(index)
            .OnMatchWaitForAnother(0.1f)
            .WithCancelingThrough("<Keyboard>/Escape")
            .OnCancel(_ => OnRebind(action, callback))
            .OnComplete(_ => OnRebind(action, callback)).Start();
    }

    private void OnRebind(InputAction action, Action callback)
    {
        rebindingOperation.Dispose();
        rebindingOperation = null;
        action.Enable();
        var rebinds = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        callback();
    }
}
