using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectDisplayManager : DisplayManager
{
    public List<GameObject> gameObjects;

    private List<bool> initialActivationStates;

    private void Awake()
    {
        initialActivationStates = gameObjects.Select(x => x.activeSelf).ToList();
    }

    public override void HighlightLinked()
    {
    }

    public override void HighlightLinkedUnder()
    {
        gameObjects.ForEach(x => x.SetActive(false));
    }

    public override void HighlightSignal()
    {
    }

    public override void HighlightSignalUnder()
    {
        gameObjects.ForEach(x => x.SetActive(false));
    }

    public override void HighlightUnlinked()
    {
    }

    public override void HighlightUnlinkedUnder()
    {
        gameObjects.ForEach(x => x.SetActive(false));
    }

    public override void Invisible()
    {
        gameObjects.ForEach(x => x.SetActive(false));
    }

    public override void Reset()
    {
        for(int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].SetActive(initialActivationStates[i]);
        }
    }

    public override void Select()
    {
    }

    public override void Hover()
    {
    }

    public override void Shadow()
    {
        gameObjects.ForEach(x => x.SetActive(false));
    }

    public override void Transparent()
    {
        gameObjects.ForEach(x => x.SetActive(false));
    }
}
