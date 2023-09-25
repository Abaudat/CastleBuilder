using System;
using UnityEngine;

public class EditModeAutosave : MonoBehaviour
{
    public static string AUTOSAVE_PLAYER_PREFS_KEY = "Autosave";

    private CubeGridPersistenceManager cubeGridPersistenceManager;

    private void Awake()
    {
        cubeGridPersistenceManager = FindObjectOfType<CubeGridPersistenceManager>();
    }

    private void Start()
    {
        CubeGrid.ElementReplaced += AutosaveEventHandler;
        CubeGrid.ElementRotated += AutosaveEventHandler;
        CubeGrid.ElementConsumerAdded += AutosaveEventHandler;
        CubeGrid.ElementConsumerRemoved += AutosaveEventHandler;
    }

    private void AutosaveEventHandler(object sender, EventArgs eventArgs)
    {
        AutosaveLevel();
    }

    private void AutosaveLevel()
    {
        PlayerPrefs.SetString(AUTOSAVE_PLAYER_PREFS_KEY, cubeGridPersistenceManager.Export());
    }
}
