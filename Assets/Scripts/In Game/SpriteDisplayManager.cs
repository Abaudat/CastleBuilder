using UnityEngine;

public class SpriteDisplayManager : DisplayManager
{
    private Color initialColor;
    private new SpriteRenderer renderer;
    private ColorDatabase colorDatabase;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        initialColor = renderer.color;
        colorDatabase = FindObjectOfType<ColorDatabase>();
    }

    public override void HighlightLinked()
    {
    }

    public override void HighlightLinkedUnder()
    {
        renderer.color = colorDatabase.invisibleColor;
    }

    public override void HighlightSignal()
    {
    }

    public override void HighlightSignalUnder()
    {
        renderer.color = colorDatabase.invisibleColor;
    }

    public override void HighlightUnlinked()
    {
    }

    public override void HighlightUnlinkedUnder()
    {
        renderer.color = colorDatabase.invisibleColor;
    }

    public override void Invisible()
    {
        renderer.color = colorDatabase.invisibleColor;
    }

    public override void Reset()
    {
        renderer.color = initialColor;
    }

    public override void Select()
    {
    }

    public override void Hover()
    {
    }

    public override void Shadow()
    {
        renderer.color = colorDatabase.invisibleColor;
    }

    public override void Transparent()
    {
        renderer.color = colorDatabase.invisibleColor;
    }
}
