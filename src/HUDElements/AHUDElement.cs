using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public abstract class AHUDElement
{
    public abstract string Name { get; }
    
    public abstract string SpritePath { get; }
    public virtual bool Visible { get; private set; } = false;

    public abstract float X { get; }
    public abstract float Y { get; }
    public virtual float Z { get; private set; } = 0f;
    public GameObject GameObject { get; set; } //Assigned at Runtime
    public SpriteRenderer SpriteRenderer  => GameObject.GetComponent<SpriteRenderer>(); //Assigned at Runtime

    public virtual void SetVisibility(bool visibility)
    {
        Visible = visibility;
        SpriteRenderer.enabled = visibility;
    }
}