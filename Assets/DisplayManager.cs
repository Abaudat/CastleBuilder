using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DisplayManager : MonoBehaviour
{
    public abstract void Reset();

    public abstract void Select();

    public abstract void Shadow();

    public abstract void HighlightSignal();

    public abstract void HighlightLinked();

    public abstract void HighlightUnlinked();

    public abstract void HighlightSignalUnder();

    public abstract void HighlightLinkedUnder();

    public abstract void HighlightUnlinkedUnder();

    public abstract void Transparent();

    public abstract void Invisible();
}
