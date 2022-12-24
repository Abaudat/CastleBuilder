using UnityEngine;

public class MaterialManager : MonoBehaviour
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

    public void ResetMaterial()
    {
        renderer.material = initialMaterial;
    }

    public void Select()
    {
        renderer.material = materialDatabase.selectMaterial;
    }

    public void Shadow()
    {
        renderer.material = materialDatabase.shadowMeterial;
    }

    public void HighlightSignal()
    {
        renderer.material = materialDatabase.signalHighlightMaterial;
    }

    public void HighlightLinked()
    {
        renderer.material = materialDatabase.signalLinkedConsumerMaterial;
    }

    public void HighlightUnlinked()
    {
        renderer.material = materialDatabase.signalUnlinkedConsumerMaterial;
    }

    public void HighlightSignalUnder()
    {
        renderer.material = materialDatabase.signalHighlightUnderMaterial;
    }

    public void HighlightLinkedUnder()
    {
        renderer.material = materialDatabase.signalLinkedUnder;
    }

    public void HighlightUnlinkedUnder()
    {
        renderer.material = materialDatabase.signalUnlinkedUnder;
    }

    public void Transparent()
    {
        renderer.material = materialDatabase.transparentMaterial;
    }

    public void Invisible()
    {
        renderer.material = materialDatabase.invisibleMaterial;
    }
}
