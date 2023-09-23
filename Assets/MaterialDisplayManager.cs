using UnityEngine;

public class MaterialDisplayManager : DisplayManager
{
    private Material initialMaterial;
    private new Renderer renderer;
    private MaterialDatabase materialDatabase;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        initialMaterial = renderer.material;
        materialDatabase = FindObjectOfType<MaterialDatabase>();
    }

    public override void Reset()
    {
        renderer.material = initialMaterial;
    }

    public override void Select()
    {
        renderer.material = materialDatabase.selectMaterial;
    }

    public override void Hover()
    {
        renderer.material = materialDatabase.hoverMaterial;
    }

    public override void Shadow()
    {
        renderer.material = materialDatabase.shadowMeterial;
    }

    public override void HighlightSignal()
    {
        renderer.material = materialDatabase.signalHighlightMaterial;
    }

    public override void HighlightLinked()
    {
        renderer.material = materialDatabase.signalLinkedConsumerMaterial;
    }

    public override void HighlightUnlinked()
    {
        renderer.material = materialDatabase.signalUnlinkedConsumerMaterial;
    }

    public override void HighlightSignalUnder()
    {
        renderer.material = materialDatabase.signalHighlightUnderMaterial;
    }

    public override void HighlightLinkedUnder()
    {
        renderer.material = materialDatabase.signalLinkedUnder;
    }

    public override void HighlightUnlinkedUnder()
    {
        renderer.material = materialDatabase.signalUnlinkedUnder;
    }

    public override void Transparent()
    {
        renderer.material = materialDatabase.transparentMaterial;
    }

    public override void Invisible()
    {
        renderer.material = materialDatabase.invisibleMaterial;
    }
}
